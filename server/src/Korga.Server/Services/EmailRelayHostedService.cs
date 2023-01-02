using Korga.Server.ChurchTools;
using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.Database.EmailRelay;
using Korga.Server.Extensions;
using Korga.Server.Models;
using Korga.Server.Utilities;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Services;

public class EmailRelayHostedService : RepeatedExecutionService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly IServiceProvider serviceProvider;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger, IServiceProvider serviceProvider)
        : base(logger)
    {
        this.options = options;
        this.serviceProvider = serviceProvider;

        Interval = TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes);
    }

    protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
    {
        using IServiceScope serviceScope = serviceProvider.CreateScope();
        DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        DistributionListService distributionList = serviceScope.ServiceProvider.GetRequiredService<DistributionListService>();

        await RetrieveAndSaveMessages(database, stoppingToken);

        await FetchRecipients(database, distributionList, stoppingToken);

        await DeliverToRecipients(database, stoppingToken);
    }

    private async ValueTask RetrieveAndSaveMessages(DatabaseContext database, CancellationToken stoppingToken)
    {
        using ImapClient imap = new();
        await imap.ConnectAsync(options.Value.ImapHost, options.Value.ImapPort, options.Value.ImapUseSsl, stoppingToken);
        await imap.AuthenticateAsync(options.Value.ImapUsername, options.Value.ImapPassword, stoppingToken);
        await imap.Inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);
        logger.LogDebug("Opened IMAP inbox with {MessageCount} messages", imap.Inbox.Count);

        MessageSummaryItems fetchItems =
            MessageSummaryItems.UniqueId | MessageSummaryItems.Flags | MessageSummaryItems.Headers | MessageSummaryItems.Body;

        IList<IMessageSummary> messages = await imap.Inbox.FetchAsync(min: 0, max: -1, fetchItems, stoppingToken);

        foreach (IMessageSummary message in messages)
        {
            bool messageAlreadyDownloaded = message.Flags.GetValueOrDefault().HasFlag(MessageFlags.Seen);
            if (messageAlreadyDownloaded) continue;

            byte[] bodyContent;

            // Dispose body and memoryStream directly after use to limit memory consumption
            using (MimeEntity body = await imap.Inbox.GetBodyPartAsync(message.UniqueId, message.Body, stoppingToken))
            using (System.IO.MemoryStream memoryStream = new())
            {
                // Writing to a MemoryStream is a synchronous operation that won't be cancelled anyhow
                body.WriteTo(memoryStream, CancellationToken.None);
                bodyContent = memoryStream.ToArray();
            }

            string from = message.Headers[HeaderId.From];
            string to = message.Headers[HeaderId.To];
            string? receiver = message.Headers.GetReceiver();

            Email emailEntity = new(
                subject: message.Headers[HeaderId.Subject],
                from: from,
                sender: message.Headers[HeaderId.Sender],
                to: to,
                receiver: receiver,
                body: bodyContent);

            // Skip next stage if receiver could not be determined
            if (receiver == null)
                emailEntity.RecipientsFetchTime = DateTime.UtcNow;

            database.Emails.Add(emailEntity);

            await database.SaveChangesAsync(stoppingToken);

            // Don't cancel this operation because messages would sent twice otherwise
            await imap.Inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, CancellationToken.None);

            if (receiver != null)
            {
                logger.LogInformation("Downloaded and stored message #{Id} from {From} for {Receiver}", emailEntity.Id, from, receiver);
            }
            else
            {
                logger.LogWarning("Could not determine receiver for message #{Id} from {From} to {To}. This message will not be forwarded." +
                    "Please make sure your email provider specifies the receiver in the Received, Envelope-To, or X-Envelope-To header", emailEntity.Id, from, to);
            }
        }

        await imap.DisconnectAsync(quit: true, stoppingToken);
    }

    private async ValueTask FetchRecipients(DatabaseContext database, DistributionListService distributionList, CancellationToken stoppingToken)
    {
        List<Email> retrieved =
            await database.Emails.Where(m => m.Receiver != null && m.RecipientsFetchTime == default).ToListAsync(stoppingToken);

        foreach (Email email in retrieved)
        {
            int atIdx = email.Receiver!.IndexOf('@');
            string emailAlias = email.Receiver!.Remove(atIdx);

            Group? group = await distributionList.GetGroupForAlias(emailAlias, stoppingToken);

            if (group != null)
            {
                EmailRecipient[] recipients = await distributionList.GetRecipientsForGroup(group, email.Id, stoppingToken);
                database.EmailRecipients.AddRange(recipients);
                email.DistributionListType = DistributionListType.CTGroup;
                email.RecipientsFetchTime = DateTime.UtcNow;
                await database.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Fetched {RecipientsCount} recipients for email #{Id} to {Receiver}", recipients.Length, email.Id, email.Receiver);
            }
            else // In case of an invalid alias DistributionListType is None. An error email will be sent to the sender at the next stage.
            {
                if (MailboxAddress.TryParse(email.From, out MailboxAddress fromAddress))
                {
                    string errorMessage = $"Hallo {fromAddress.Name},\r\ndeine E-Mail mit dem Betreff {email.Subject} an {email.Receiver} konnte nicht zugestellt werden. " +
                        "Die E-Mail-Adresse ist ungültig.";

                    database.EmailRecipients.Add(new(fromAddress.Address, fromAddress.Name) { EmailId = email.Id, ErrorMessage = errorMessage });
                }

                email.RecipientsFetchTime = DateTime.UtcNow;
                await database.SaveChangesAsync(stoppingToken);

                logger.LogInformation("No group found with alias {Receiver} for email #{Id} from {From}", email.Receiver, email.Id, email.From);
            }
        }
    }

    private async ValueTask DeliverToRecipients(DatabaseContext database, CancellationToken stoppingToken)
    {
        using SmtpClient smtpClient = new();
        await smtpClient.ConnectAsync(options.Value.SmtpHost, options.Value.SmtpPort, options.Value.SmtpUseSsl, stoppingToken);
        await smtpClient.AuthenticateAsync(options.Value.SmtpUsername, options.Value.SmtpPassword, stoppingToken);

        // Get emails one by one for lower memory usage
        while (!stoppingToken.IsCancellationRequested)
        {
            Email? pending = await
                (from m in database.Emails.Where(m => m.RecipientsFetchTime != default)
                 join r in database.EmailRecipients.Where(r => r.DeliveryTime == default) on m.Id equals r.EmailId
                 orderby m.Id
                 select m)
                 .FirstOrDefaultAsync(stoppingToken);

            if (pending == null) break;

            MimeEntity body;
            using (System.IO.MemoryStream memoryStream = new(pending.Body))
                body = MimeEntity.Load(memoryStream);

            List<EmailRecipient> recipients = await
                database.EmailRecipients.Where(r => r.EmailId == pending.Id && r.DeliveryTime == default).ToListAsync(stoppingToken);

            logger.LogInformation("Delivering email #{Id} to {RecipientsCount} recipients", pending.Id, recipients.Count);

            foreach (EmailRecipient recipient in recipients)
            {
                MimeMessage mimeMessage = new();
                mimeMessage.To.Add(new MailboxAddress(recipient.FullName, recipient.EmailAddress));

                if (recipient.ErrorMessage == null)
                {
                    mimeMessage.From.Add(MailboxAddress.Parse(pending.From));
                    mimeMessage.Sender = new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress);
                    mimeMessage.Subject = pending.Subject;
                    mimeMessage.Body = body;
                }
                else
                {
                    mimeMessage.From.Add(new MailboxAddress(options.Value.SenderName, options.Value.SenderName));
                    mimeMessage.Subject = "Unzustellbar: " + pending.Subject;
                    mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = recipient.ErrorMessage };
                }

                try
                {
                    await smtpClient.SendAsync(mimeMessage, stoppingToken);
                }
                catch (SmtpCommandException ex) when (ex.StatusCode == SmtpStatusCode.MailboxBusy)
                {
                    // We have to handle the case of an SMTP rate limit when multiple customers are supposed to receive a mail at the same time
                    // Strato for reference only allows you to send 50 emails without delay (September 2021)
                    //
                    // RFC 821 defines common status code as 450 mailbox unavailable (busy or blocked for policy reasons)
                    // RFC 3463 defines enhanced status code as 4.7.X for persistent transient failures caused by security or policy status

                    logger.LogInformation("Mailbox busy. This is most likely caused by a temporary rate limit.");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Sending email #{Id} to {FullName} <{EmailAddress}> failed",
                        pending.Id, recipient.FullName, recipient.EmailAddress);
                    break;
                }

                recipient.DeliveryTime = DateTime.UtcNow;

                // Don't cancel this operation because messages would sent twice otherwise
                await database.SaveChangesAsync(CancellationToken.None);

                logger.LogInformation("Delivered email #{Id} to {FullName} <{EmailAddress}>",
                    pending.Id, recipient.FullName, recipient.EmailAddress);
            }
        }

        await smtpClient.DisconnectAsync(quit: true, stoppingToken);
    }
}
