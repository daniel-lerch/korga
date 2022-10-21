using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Services;

public class EmailRelayHostedService : RepeatedExecutionService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly ILogger<EmailRelayHostedService> logger;
    private readonly IServiceProvider serviceProvider;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger, IServiceProvider serviceProvider)
        : base(TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes))
    {
        this.options = options;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
    {
        using (IServiceScope serviceScope = serviceProvider.CreateScope())
        using (ImapClient imap = new())
        {
            DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

            await imap.ConnectAsync(options.Value.ImapHost, options.Value.ImapPort, options.Value.ImapUseSsl, stoppingToken);
            await imap.AuthenticateAsync(options.Value.ImapUsername, options.Value.ImapPassword, stoppingToken);
            await imap.Inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);
            logger.LogInformation("Opened IMAP inbox with {MessageCount} messages", imap.Inbox.Count);

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

                database.Emails.Add(new Email(
                    subject: message.Headers[HeaderId.Subject],
                    from: message.Headers[HeaderId.From],
                    sender: message.Headers[HeaderId.Sender],
                    to: message.Headers[HeaderId.To],
                    receiver: receiver,
                    body: bodyContent));

                await database.SaveChangesAsync(stoppingToken);

                // Don't cancel this operation because messages would sent twice otherwise
                await imap.Inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, CancellationToken.None);

                logger.LogInformation("Downloaded and stored message from {From} for {Receiver}", message.Headers[HeaderId.From], receiver);
            }
        }
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
}
