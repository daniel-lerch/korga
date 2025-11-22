using ChurchTools;
using Korga.EmailRelay.Entities;
using MimeKit;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.EmailRelay;

public class DistributionListService
{
    private readonly IChurchToolsApi churchTools;

    public DistributionListService(IChurchToolsApi churchTools)
    {
        this.churchTools = churchTools;
    }

    public async ValueTask<MailboxAddress[]> GetRecipients(DistributionList distributionList, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(distributionList.RecipientsQuery)) return [];

        var people = await churchTools.ChurchQuery<IdNameEmail>(distributionList.RecipientsQuery, cancellationToken);

        return people
            .Where(p => !string.IsNullOrWhiteSpace(p.Email))
        // Avoid duplicate emails for married couples with a shared email address
            .GroupBy(p => p.Email)
            .Select(grouping => new MailboxAddress(
                name: string.Join(", ", grouping.Select(r => r.FirstName)) + ' ' + grouping.First().LastName,
                address: grouping.Key))
            .ToArray();
    }
}
