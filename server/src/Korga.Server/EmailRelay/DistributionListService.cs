using Korga.ChurchTools.Entities;
using Korga.EmailRelay.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class DistributionListService
{
	private readonly DatabaseContext database;

	public DistributionListService(DatabaseContext database)
	{
		this.database = database;
	}

	public async ValueTask<EmailRecipient[]> GetRecipients(DistributionList distributionList, long emailId, CancellationToken cancellationToken)
	{
		List<PersonFilter> personFilters = await database.PersonFilters.Where(pf => pf.DistributionListId == distributionList.Id).ToListAsync(cancellationToken);

		List<Person> recipients = new();

		foreach (PersonFilter personFilter in personFilters)
		{
			if (personFilter is GroupFilter groupFilter)
			{
				recipients.AddRange(
					await database.GroupMembers
						.Where(m => m.GroupId == groupFilter.GroupId && (groupFilter.GroupRoleId == null || m.GroupRoleId == groupFilter.GroupRoleId))
						.Join(database.People, m => m.PersonId, p => p.Id, (m, p) => p)
						.ToListAsync(cancellationToken));
			}
			else if (personFilter is StatusFilter statusFilter)
			{
				recipients.AddRange(
					await database.People
						.Where(p => p.StatusId == statusFilter.StatusId && !string.IsNullOrEmpty(p.Email))
						.ToListAsync(cancellationToken));
			}
		}

		// Avoid duplicate emails for married couples with a shared email address
		return recipients
			.GroupBy(p => p.Email)
			.Select(grouping => new EmailRecipient(
				emailAddress: grouping.Key,
				fullName: string.Join(", ", grouping.Select(r => r.FirstName)) + ' ' + grouping.First().LastName)
			{ EmailId = emailId })
			.ToArray();
	}
}
