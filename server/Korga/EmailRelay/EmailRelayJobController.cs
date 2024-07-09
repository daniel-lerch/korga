using Korga.EmailDelivery;
using Korga.EmailRelay.Entities;
using Korga.Filters;
using Korga.Filters.Entities;
using Korga.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.EmailRelay;

public class EmailRelayJobController : OneAtATimeJobController<InboxEmail>
{
    private readonly DatabaseContext database;
    private readonly ILogger<EmailRelayJobController> logger;
    private readonly DistributionListService distributionListService;
    private readonly MimeMessageCreationService errorMessage;
    private readonly PersonFilterService filterService;
    private readonly EmailDeliveryService emailDelivery;

    public EmailRelayJobController(DatabaseContext database, ILogger<EmailRelayJobController> logger, DistributionListService distributionListService, MimeMessageCreationService errorMessage, PersonFilterService filterService, EmailDeliveryService emailDelivery)
    {
        this.database = database;
        this.logger = logger;
        this.distributionListService = distributionListService;
        this.errorMessage = errorMessage;
        this.filterService = filterService;
        this.emailDelivery = emailDelivery;
    }

    protected override async ValueTask<InboxEmail?> NextPendingOrDefault(CancellationToken cancellationToken)
    {
        return await database.InboxEmails
            .Where(email => email.ProcessingCompletedTime == default)
            .OrderBy(email => email.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected override async ValueTask ExecuteJob(InboxEmail email, CancellationToken cancellationToken)
    {
        if (email.Receiver == null)
        {
            using MimeMessage? mimeMessage = errorMessage.InvalidServerConfiguration(email);
            await SendErrorMessage(email, mimeMessage, cancellationToken);

            logger.LogWarning("Could not determine receiver for message #{Id} from {From} to {To}. This message will not be forwarded." +
                "Please make sure your email provider specifies the receiver in the Received, Envelope-To, or X-Envelope-To header", email.Id, email.From, email.To);
            return;
        }

        int atIdx = email.Receiver.IndexOf('@');
        string emailAlias = email.Receiver.Remove(atIdx);

        DistributionList? distributionList = await database.DistributionLists.SingleOrDefaultAsync(x => x.Alias == emailAlias, cancellationToken);

        if (distributionList == null)
        {
            using MimeMessage? mimeMessage = errorMessage.InvalidAlias(email);
            await SendErrorMessage(email, mimeMessage, cancellationToken);

            logger.LogInformation("No group found with alias {Receiver} for email #{Id} from {From}", email.Receiver, email.Id, email.From);
            return;
        }

        if (!await IsSenderPermitted(email, distributionList, cancellationToken))
        {
            await SendErrorMessage(email, errorMessage.SenderNotPermitted(email), cancellationToken);

            logger.LogInformation("Sender {From}, {Sender} is not permitted to send to distribution list {Receiver} for email #{Id}", email.From, email.Sender, email.Receiver, email.Id);
            return;
        }

        if (email.Header == null)
        {
            using MimeMessage? mimeMessage = errorMessage.TooManyHeaders(email);
            await SendErrorMessage(email, mimeMessage, cancellationToken);

            logger.LogInformation("Email #{Id} from {From} to {Receiver} exceeded the header size limit", email.Id, email.From, email.Receiver);
            return;
        }

        if (email.Body == null)
        {
            using MimeMessage? mimeMessage = errorMessage.TooBigMessage(email);
            await SendErrorMessage(email, mimeMessage, cancellationToken);

            logger.LogInformation("Email #{Id} from {From} to {Receiver} exceeded the body size limit", email.Id, email.From, email.Receiver);
            return;
        }

        MailboxAddress[] recipients = await distributionListService.GetRecipients(distributionList, cancellationToken);
        foreach (MailboxAddress address in recipients)
        {
            using MimeMessage preparedMessage = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter)
                ? await errorMessage.PrepareForForwardTo(email, address, cancellationToken)
                : errorMessage.PrepareForResentTo(email, address);
            await emailDelivery.Enqueue(address.Address, preparedMessage, email.Id, cancellationToken);
        }
        email.DistributionListId = distributionList.Id;
        email.ProcessingCompletedTime = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fetched {RecipientsCount} recipients for email #{Id} to {Receiver}", recipients.Length, email.Id, email.Receiver);
        return;
    }

    private async ValueTask<bool> IsSenderPermitted(InboxEmail email, DistributionList distributionList, CancellationToken cancellationToken)
    {
        List<PersonFilter> permittedSenders = await database.PersonFilters
            .Where(f => f.PersonFilterListId == distributionList.PermittedSendersId)
            .ToListAsync(cancellationToken);

        // If no permitted senders are defined, everyone is allowed to send to this distribution list
        if (permittedSenders.Count == 0) return true;

        string? sender = GetActualSender(email);
        if (sender == null) return false;

        List<int> senderPersonIds = await database.People.Where(p => p.Email == sender).Select(p => p.Id).ToListAsync(cancellationToken);

        foreach (PersonFilter filter in permittedSenders)
        {
            if (await filterService.FilterToQuery(filter).AnyAsync(p => senderPersonIds.Contains(p.Id), cancellationToken))
                return true;
        }

        return false;
    }

    private async ValueTask SendErrorMessage(InboxEmail email, MimeMessage? errorMessage, CancellationToken cancellationToken)
    {
        if (errorMessage != null)
            await emailDelivery.Enqueue(((MailboxAddress)errorMessage.To[0]).Address, errorMessage, email.Id, cancellationToken);

        email.ProcessingCompletedTime = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
    }

    private static string? GetActualSender(InboxEmail email)
    {
        if (email.Sender != null)
        {
            if (MailboxAddress.TryParse(email.Sender, out MailboxAddress mailboxAddress))
                return mailboxAddress.Address;
            else
                return null;
        }
        else
        {
            return MailboxAddressHelper.FirstMailboxAddressOrDefault(email.From)?.Address;
        }
    }
}
