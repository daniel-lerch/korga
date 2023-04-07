using Korga.EmailRelay.Entities;
using Korga.Server.Extensions;
using Korga.Server.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class ImapReceiverService
{
    private readonly ILogger<ImapReceiverService> logger;
    private readonly IOptions<EmailRelayOptions> options;
    private readonly DatabaseContext database;
    private readonly JobQueue<EmailRelayJobController> jobQueue;

    public ImapReceiverService(ILogger<ImapReceiverService> logger, IOptions<EmailRelayOptions> options, DatabaseContext database, JobQueue<EmailRelayJobController> jobQueue)
    {
        this.logger = logger;
        this.options = options;
        this.database = database;
        this.jobQueue = jobQueue;
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
            if (message.Flags.GetValueOrDefault().HasFlag(MessageFlags.Seen)) continue;

            // Check if message has been downloaded but not marked as seen
            if (await database.InboxEmails.AnyAsync(email => email.UniqueId == message.UniqueId.Id, stoppingToken))
            {
                await imap.Inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, stoppingToken);
                continue;
            }

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

            database.InboxEmails.Add(emailEntity);

            await database.SaveChangesAsync(stoppingToken);

            jobQueue.EnsureRunning();

            await imap.Inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, stoppingToken);

            logger.LogInformation("Downloaded and stored message #{Id} from {From} for {Receiver}", emailEntity.Id, from, receiver ?? "an unkown receiver");
        }

        await imap.DisconnectAsync(quit: true, stoppingToken);
    }
}
