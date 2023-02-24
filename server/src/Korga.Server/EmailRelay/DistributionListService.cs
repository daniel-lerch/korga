﻿using Korga.ChurchTools.Entities;
using Korga.EmailRelay.Entities;
using Korga.Extensions;
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
		return (await GetPeople(distributionList, cancellationToken))
		// Avoid duplicate emails for married couples with a shared email address
			.GroupBy(p => p.Email)
			.Select(grouping => new EmailRecipient(
				emailAddress: grouping.Key,
				fullName: string.Join(", ", grouping.Select(r => r.FirstName)) + ' ' + grouping.First().LastName)
			{ EmailId = emailId })
			.ToArray();
	}

	public async ValueTask<IEnumerable<Person>> GetPeople(DistributionList distributionList, CancellationToken cancellationToken)
	{
		List<PersonFilter> personFilters = await database.PersonFilters.Where(f => f.DistributionListId == distributionList.Id).ToListAsync(cancellationToken);
		
		HashSet<Person> people = new();

		foreach (PersonFilter personFilter in personFilters)
		{
			if (personFilter is GroupFilter groupFilter)
			{
				people.AddRange(
					await database.GroupMembers
						.Where(m => m.GroupId == groupFilter.GroupId && (groupFilter.GroupRoleId == null || m.GroupRoleId == groupFilter.GroupRoleId))
						.Join(database.People, m => m.PersonId, p => p.Id, (m, p) => p)
						.ToListAsync(cancellationToken));
			}
			else if (personFilter is StatusFilter statusFilter)
			{
				people.AddRange(
					await database.People
						.Where(p => p.StatusId == statusFilter.StatusId && !string.IsNullOrEmpty(p.Email))
						.ToListAsync(cancellationToken));
			}
			else if (personFilter is SinglePerson singlePerson)
			{
				people.Add(
					await database.People.SingleAsync(p => p.Id == singlePerson.PersonId, cancellationToken));
			}
		}

		return people;
	}
}
