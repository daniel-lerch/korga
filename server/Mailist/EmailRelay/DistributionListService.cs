using Mailist.EmailRelay.Entities;
using Mailist.Filters;
using MimeKit;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.EmailRelay;

public class DistributionListService
{
    private readonly PersonFilterService filterService;

    public DistributionListService(PersonFilterService filterService)
    {
        this.filterService = filterService;
    }

    public async ValueTask<MailboxAddress[]> GetRecipients(DistributionList distributionList, CancellationToken cancellationToken)
    {
        if (!distributionList.PermittedRecipientsId.HasValue) return [];

        return (await filterService.GetPeople(distributionList.PermittedRecipientsId.Value, cancellationToken))
            .Where(p => !string.IsNullOrWhiteSpace(p.Email))
        // Avoid duplicate emails for married couples with a shared email address
            .GroupBy(p => p.Email)
            .Select(grouping => new MailboxAddress(
                name: string.Join(", ", grouping.Select(r => r.FirstName)) + ' ' + grouping.First().LastName,
                address: grouping.Key))
            .ToArray();
    }
}
