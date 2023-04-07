using Korga.EmailRelay.Entities;
using Korga.Server.EmailDelivery;
using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class EmailRelayJobController : OneAtATimeJobController<InboxEmail>
{
    private readonly DatabaseContext database;
    private readonly ILogger<EmailRelayJobController> logger;
    private readonly DistributionListService distributionListService;
    private readonly MimeMessageCreationService emailRelay;
    private readonly EmailDeliveryService emailDelivery;

    public EmailRelayJobController(DatabaseContext database, ILogger<EmailRelayJobController> logger, DistributionListService distributionListService, MimeMessageCreationService emailRelay, EmailDeliveryService emailDelivery)
    {
        this.database = database;
        this.logger = logger;
        this.distributionListService = distributionListService;
        this.emailRelay = emailRelay;
        this.emailDelivery = emailDelivery;
    }

    protected override async ValueTask<InboxEmail?> NextPendingOrDefault(CancellationToken cancellationToken)
    {
        return await database.InboxEmails.FirstOrDefaultAsync(email => email.ProcessingCompletedTime == default, cancellationToken);
    }

    protected override async ValueTask ExecuteJob(InboxEmail email, CancellationToken cancellationToken)
    {
        if (email.Receiver == null)
        {
            MimeMessage? errorMessage = emailRelay.InvalidServerConfiguration(email);
            if (errorMessage != null)
                await emailDelivery.Enqueue(((MailboxAddress)errorMessage.To[0]).Address, errorMessage, email.Id, cancellationToken);

            logger.LogWarning("Could not determine receiver for message #{Id} from {From} to {To}. This message will not be forwarded." +
                "Please make sure your email provider specifies the receiver in the Received, Envelope-To, or X-Envelope-To header", email.Id, email.From, email.To);

            email.ProcessingCompletedTime = DateTime.UtcNow;
            await database.SaveChangesAsync(cancellationToken);

            return true;
        }

        int atIdx = email.Receiver.IndexOf('@');
        string emailAlias = email.Receiver.Remove(atIdx);

        DistributionList? distributionList = await database.DistributionLists.SingleOrDefaultAsync(x => x.Alias == emailAlias, cancellationToken);

        if (distributionList == null)
        {
            MimeMessage? errorMessage = emailRelay.InvalidAlias(email);
            if (errorMessage != null)
                await emailDelivery.Enqueue(((MailboxAddress)errorMessage.To[0]).Address, errorMessage, email.Id, cancellationToken);

            logger.LogInformation("No group found with alias {Receiver} for email #{Id} from {From}", email.Receiver, email.Id, email.From);

            email.ProcessingCompletedTime = DateTime.UtcNow;
            await database.SaveChangesAsync(cancellationToken);

            return true;
        }

        string[] recipients = await distributionListService.GetRecipients(distributionList, cancellationToken);
        foreach (string address in recipients)
        {
            MimeMessage preparedMessage = emailRelay.PrepareForResentTo(email, MailboxAddress.Parse(address));
            await emailDelivery.Enqueue(address, preparedMessage, email.Id, cancellationToken);
        }
        email.DistributionListId = distributionList.Id;
        email.ProcessingCompletedTime = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fetched {RecipientsCount} recipients for email #{Id} to {Receiver}", recipients.Length, email.Id, email.Receiver);
        return true;
    }
}
