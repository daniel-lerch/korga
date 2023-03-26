using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

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
        ImapReceiverService imapService = serviceScope.ServiceProvider.GetRequiredService<ImapReceiverService>();
        DistributionListService distributionListService = serviceScope.ServiceProvider.GetRequiredService<DistributionListService>();
        EmailDeliveryService emailDelivery = serviceScope.ServiceProvider.GetRequiredService<EmailDeliveryService>();

        await imapService.FetchAsync(stoppingToken);

        List<InboxEmail> retrieved =
            await database.InboxEmails.Where(m => m.Receiver != null && m.ProcessingCompletedTime == default).ToListAsync(stoppingToken);

        foreach (InboxEmail email in retrieved)
        {
            int atIdx = email.Receiver!.IndexOf('@');
            string emailAlias = email.Receiver!.Remove(atIdx);

            DistributionList? distributionList = await database.DistributionLists.SingleOrDefaultAsync(x => x.Alias == emailAlias, stoppingToken);

            if (distributionList != null)
            {
                string[] recipients = await distributionListService.GetRecipients(distributionList, stoppingToken);
                // TODO: Prepare and enqueue message for every recipient
                email.DistributionListId = distributionList.Id;
                email.ProcessingCompletedTime = DateTime.UtcNow;
                await database.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Fetched {RecipientsCount} recipients for email #{Id} to {Receiver}", recipients.Length, email.Id, email.Receiver);
            }
            else // In case of an invalid alias DistributionListType is None. An error email will be sent to the sender at the next stage.
            {
                //if (MailboxAddress.TryParse(email.From, out MailboxAddress fromAddress))
                //{
                //    string errorMessage = $"Hallo {fromAddress.Name},\r\ndeine E-Mail mit dem Betreff {email.Subject} an {email.Receiver} konnte nicht zugestellt werden. " +
                //        "Die E-Mail-Adresse ist ungültig.";

                //    database.OutboxEmails.Add(new(fromAddress.Address, fromAddress.Name) { InboxEmailId = email.Id, ErrorMessage = errorMessage });
                //}

                email.ProcessingCompletedTime = DateTime.UtcNow;
                await database.SaveChangesAsync(stoppingToken);

                logger.LogInformation("No group found with alias {Receiver} for email #{Id} from {From}", email.Receiver, email.Id, email.From);
            }
        }

    }
}
