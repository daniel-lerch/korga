using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.Database.Entities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Services;

public class EmailRelayHostedService : BackgroundService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly ILogger<EmailRelayHostedService> logger;
    private readonly IServiceProvider serviceProvider;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger, IServiceProvider serviceProvider)
    {
        this.options = options;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
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

                        database.Emails.Add(new Email(
                            subject: message.Headers[HeaderId.Subject],
                            from: message.Headers[HeaderId.From],
                            sender: message.Headers[HeaderId.Sender],
                            to: message.Headers[HeaderId.To],
                            receiver: GetReceiver(message.Headers),
                            body: bodyContent));

                        await database.SaveChangesAsync(stoppingToken);

                        // Don't cancel this operation because messages would sent twice otherwise
                        await imap.Inbox.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, CancellationToken.None);

                        logger.LogInformation("Downloaded and stored message from {From}", message.Headers[HeaderId.From]);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

    }

    private string? GetReceiver(HeaderList headers)
    {
        logger.LogInformation(string.Join(",\r\n", headers.Select(x => $"{x.Field}: {x.Value}")));
        return null;
    }
}
