using Korga.Server.ChurchTools;
using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.Database.Entities;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Services;

public class EmailRelayHostedService : RepeatedExecutionService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly ChurchToolsApiService churchTools;
    private readonly IServiceProvider serviceProvider;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger, ChurchToolsApiService churchTools, IServiceProvider serviceProvider)
        : base(logger)
    {
        this.options = options;
        this.churchTools = churchTools;
        this.serviceProvider = serviceProvider;

        Interval = TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes);
    }

    protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
    {
        using IServiceScope serviceScope = serviceProvider.CreateScope();
        DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

        await RetrieveAndSaveMessages(database, stoppingToken);

        await FetchRecipients(database, stoppingToken);

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

        IList<IMessageSummary> messages = await imap.Inbox.FetchAsync(0, -1, fetchItems, stoppingToken);

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

            string? receiver = GetReceiver(message.Headers);

            Email emailEntity = new(
                subject: message.Headers[HeaderId.Subject],
                from: message.Headers[HeaderId.From],
                sender: message.Headers[HeaderId.Sender],
                to: message.Headers[HeaderId.To],
                receiver: receiver,
                body: bodyContent);

            database.Emails.Add(emailEntity);

            await database.SaveChangesAsync(stoppingToken);

            // Don't cancel this operation because messages would sent twice otherwise
            await imap.Inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, CancellationToken.None);

            logger.LogInformation("Downloaded and stored message {Id} from {From} for {Receiver}", emailEntity.Id, message.Headers[HeaderId.From], receiver);
        }

        await imap.DisconnectAsync(quit: true, stoppingToken);
    }

    private string? GetReceiver(HeaderList headers)
    {
        //logger.LogInformation(string.Join(",\r\n", headers.Select(x => $"{x.Field}: {x.Value}")));

        // 1. Try to get receiver from Received header
        string? receivedHeader = headers[HeaderId.Received];
        if (receivedHeader != null)
        {
            string prefix = "for <";
            string suffix = ">;";
            int prefixIdx = receivedHeader.IndexOf(prefix);
            if (prefixIdx != -1)
            {
                int endIdx = receivedHeader.IndexOf(suffix);
                if (endIdx != -1)
                {
                    int startIdx = prefixIdx + prefix.Length;
                    return receivedHeader[startIdx..endIdx];
                }
            }
            logger.LogInformation(receivedHeader);
        }

        // 2. Try to get receiver from Envelope-To or X-Envelope-To headers
        string? envelopeTo = headers["Envelope-To"] ?? headers["X-Envelope-To"];
        if (envelopeTo != null)
        {
            int prefixIdx = envelopeTo.IndexOf('<');
            if (prefixIdx != -1)
            {
                int endIdx = envelopeTo.IndexOf('>');
                if (endIdx != -1)
                {
                    return envelopeTo[(prefixIdx + 1)..endIdx];
                }
            }
        }

        return null;
    }

    private async ValueTask FetchRecipients(DatabaseContext database, CancellationToken stoppingToken)
    {
        List<Email> retrieved =
            await database.Emails.Where(m => m.RecipientsFetchTime == default).ToListAsync(stoppingToken);

        if (retrieved.Count == 0) return;

        Dictionary<string, int> groupIdForAlias = await GetGroupIdsForAliases(stoppingToken);

        foreach (Email email in retrieved)
        {
            int recipientsCount = 0;

            if (email.Receiver != null)
            {
                int atIdx = email.Receiver!.IndexOf('@');
                string emailAlias = email.Receiver!.Remove(atIdx);

                if (groupIdForAlias.TryGetValue(emailAlias, out int groupId))
                {
                    var groupMembers = await churchTools.GetGroupMembers(groupId, stoppingToken);

                    foreach (GroupMember member in groupMembers)
                    {
                        var person = await churchTools.GetPerson(member.PersonId, stoppingToken);

                        if (!string.IsNullOrEmpty(person.Email))
                        {
                            database.EmailRecipients.Add(new(person.Email, person.FirstName, person.LastName) { EmailId = email.Id });
                            recipientsCount++;
                        }
                    }
                }
            }

            email.RecipientsFetchTime = DateTime.UtcNow;
            await database.SaveChangesAsync();

            logger.LogInformation("Fetched {RecipientsCount} recipients for email {Id} to {Receiver}", recipientsCount, email.Id, email.Receiver);
        }
    }

    private async ValueTask<Dictionary<string, int>> GetGroupIdsForAliases(CancellationToken cancellationToken)
    {
        Dictionary<string, int> groupIdForAlias = new();

        var groups = await churchTools.GetGroups(cancellationToken);

        foreach (Group group in groups)
        {
            if (group.Information.TryGetValue(options.Value.ChurchToolsEmailAliasGroupField, out JsonElement emailAliasElement)
                && emailAliasElement.ValueKind == JsonValueKind.String)
            {
                // Null forgiving reason:
                // GetString() does not return null unless ValueKind is JsonValueKind.Null
                string emailAlias = emailAliasElement.GetString()!;

                if (groupIdForAlias.TryAdd(emailAlias, group.Id))
                {
                    logger.LogDebug("Group {GroupName} (#{GroupId}) has alias {EmailAlias}", group.Name, group.Id, emailAlias);
                }
                else
                {
                    Group conflict = groups.Single(g => g.Id == groupIdForAlias[emailAlias]);
                    logger.LogWarning("Group {GroupName} (#{GroupId}) has alias {EmailAlias}, the same alias like {ConflictName} (#{ConflictId})",
                        group.Name, group.Id, emailAlias, conflict.Name, conflict.Id);
                }
            }
        }

        return groupIdForAlias;
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

            logger.LogInformation("Delivering email {Id} to {RecipientsCount} recipients", pending.Id, recipients.Count);

            foreach (EmailRecipient recipient in recipients)
            {
                MimeMessage mimeMessage = new();
                mimeMessage.From.Add(MailboxAddress.Parse(pending.From));
                mimeMessage.Sender = new MailboxAddress(options.Value.SenderName, options.Value.SenderAddress);
                mimeMessage.To.Add(new MailboxAddress(recipient.GivenName, recipient.EmailAddress));
                mimeMessage.Subject = pending.Subject;
                mimeMessage.Body = body;

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
                    logger.LogError(ex, "Sending email {Id} to {GivenName} {FamilyName} <{EmailAddress}> failed",
                        pending.Id, recipient.GivenName, recipient.FamilyName, recipient.EmailAddress);
                    break;
                }

                recipient.DeliveryTime = DateTime.UtcNow;

                // Don't cancel this operation because messages would sent twice otherwise
                await database.SaveChangesAsync(CancellationToken.None);

                logger.LogInformation("Delivered email {Id} to {GivenName} {FamilyName} <{EmailAddress}>",
                    pending.Id, recipient.GivenName, recipient.FamilyName, recipient.EmailAddress);
            }
        }

        await smtpClient.DisconnectAsync(quit: true, stoppingToken);
    }
}
