using Korga.EmailRelay.Entities;
using Korga.Server.Extensions;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class ImapReceiverService
{
    private readonly ILogger<ImapReceiverService> logger;
    private readonly IOptions<EmailRelayOptions> options;
    private readonly DatabaseContext database;

    public ImapReceiverService(ILogger<ImapReceiverService> logger, IOptions<EmailRelayOptions> options, DatabaseContext database)
    {
        this.logger = logger;
        this.options = options;
        this.database = database;
    }

    public async ValueTask FetchAsync(CancellationToken stoppingToken)
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
            logger.LogInformation("UniqueId: {UniqueId}", message.UniqueId);

            bool messageAlreadyDownloaded = message.Flags.GetValueOrDefault().HasFlag(MessageFlags.Seen);
            if (messageAlreadyDownloaded) continue;

            byte[] headerContent;

            using (System.IO.MemoryStream memoryStream = new())
            {
                // Writing to a MemoryStream is a synchronous operation that won't be cancelled anyhow
                message.Headers.WriteTo(memoryStream, CancellationToken.None);
                headerContent = memoryStream.ToArray();
            }

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

            InboxEmail emailEntity = new(
                uniqueId: message.UniqueId.Id,
                subject: message.Headers[HeaderId.Subject],
                from: from,
                sender: message.Headers[HeaderId.Sender],
                to: to,
                receiver: receiver,
                header: headerContent,
                body: bodyContent);

            // Skip next stage if receiver could not be determined
            if (receiver == null)
                emailEntity.ProcessingCompletedTime = DateTime.UtcNow;

            database.InboxEmails.Add(emailEntity);

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
}
