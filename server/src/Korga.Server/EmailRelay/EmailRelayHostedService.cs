using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class EmailRelayHostedService : BackgroundService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly ILogger<EmailRelayHostedService> logger;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using ImapClient imap = new();
                await imap.ConnectAsync(options.Value.ImapHost, options.Value.ImapPort, options.Value.ImapUseSsl, stoppingToken);
                await imap.AuthenticateAsync(options.Value.ImapUsername, options.Value.ImapPassword, stoppingToken);
                await imap.Inbox.OpenAsync(FolderAccess.ReadWrite, stoppingToken);
                logger.LogInformation("Opened IMAP inbox with {MessageCount} messages", imap.Inbox.Count);

                IList<IMessageSummary> messages = await imap.Inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Body, stoppingToken);

                foreach (IMessageSummary message in messages)
                {
                    MimeEntity body = await imap.Inbox.GetBodyPartAsync(message.UniqueId, message.Body, stoppingToken);
                    logger.LogInformation(body.ToString());
                }

                await Task.Delay(TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

    }
}
