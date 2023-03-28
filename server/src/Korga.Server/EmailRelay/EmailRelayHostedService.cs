using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Korga.Server.Utilities;
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

namespace Korga.Server.EmailRelay;

public class EmailRelayHostedService : RepeatedExecutionService
{
    private readonly IServiceProvider serviceProvider;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger, IServiceProvider serviceProvider)
        : base(logger)
    {
        this.serviceProvider = serviceProvider;

        Interval = TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes);
    }

    protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
    {
        using IServiceScope serviceScope = serviceProvider.CreateScope();
        DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        ImapReceiverService imapService = serviceScope.ServiceProvider.GetRequiredService<ImapReceiverService>();
        DistributionListService distributionListService = serviceScope.ServiceProvider.GetRequiredService<DistributionListService>();
        EmailRelayService emailRelay = serviceScope.ServiceProvider.GetRequiredService<EmailRelayService>();
        EmailDeliveryService emailDelivery = serviceScope.ServiceProvider.GetRequiredService<EmailDeliveryService>();

        await imapService.FetchAsync(stoppingToken);

        List<InboxEmail> retrieved =
            await database.InboxEmails.Where(m => m.ProcessingCompletedTime == default).ToListAsync(stoppingToken);

        foreach (InboxEmail email in retrieved)
        {
            if (email.Receiver == null)
            {
                MimeMessage? errorMessage = emailRelay.InvalidServerConfiguration(email);
                if (errorMessage != null)
                    await emailDelivery.Enqueue(((MailboxAddress)errorMessage.To[0]).Address, errorMessage, email.Id, stoppingToken);

                logger.LogWarning("Could not determine receiver for message #{Id} from {From} to {To}. This message will not be forwarded." +
                    "Please make sure your email provider specifies the receiver in the Received, Envelope-To, or X-Envelope-To header", email.Id, email.From, email.To);

                email.ProcessingCompletedTime = DateTime.UtcNow;
                await database.SaveChangesAsync(stoppingToken);

                continue;
            }

            int atIdx = email.Receiver.IndexOf('@');
            string emailAlias = email.Receiver.Remove(atIdx);

            DistributionList? distributionList = await database.DistributionLists.SingleOrDefaultAsync(x => x.Alias == emailAlias, stoppingToken);

            if (distributionList == null)
            {
                MimeMessage? errorMessage = emailRelay.InvalidAlias(email);
                if (errorMessage != null)
                    await emailDelivery.Enqueue(((MailboxAddress)errorMessage.To[0]).Address, errorMessage, email.Id, stoppingToken);

                logger.LogInformation("No group found with alias {Receiver} for email #{Id} from {From}", email.Receiver, email.Id, email.From);

                email.ProcessingCompletedTime = DateTime.UtcNow;
                await database.SaveChangesAsync(stoppingToken);

                continue;
            }

            string[] recipients = await distributionListService.GetRecipients(distributionList, stoppingToken);
            foreach (string address in recipients)
            {
                MimeMessage preparedMessage = emailRelay.PrepareForResentTo(email, MailboxAddress.Parse(address));
                await emailDelivery.Enqueue(address, preparedMessage, email.Id, stoppingToken);
            }
            email.DistributionListId = distributionList.Id;
            email.ProcessingCompletedTime = DateTime.UtcNow;
            await database.SaveChangesAsync(stoppingToken);

            logger.LogInformation("Fetched {RecipientsCount} recipients for email #{Id} to {Receiver}", recipients.Length, email.Id, email.Receiver);
        }
    }
}
