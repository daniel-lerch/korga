using Korga.ChurchTools.Entities;
using Korga.EmailRelay.Entities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
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

    public async ValueTask<MailboxAddress[]> GetRecipients(DistributionList distributionList, CancellationToken cancellationToken)
    {
        if (!distributionList.PermittedRecipientsId.HasValue) return Array.Empty<MailboxAddress>();

        PersonFilter recipients = await database.PersonFilters.SingleAsync(filter => filter.Id == distributionList.PermittedRecipientsId.Value, cancellationToken);

        return (await GetPeopleRecursive(recipients, cancellationToken))
        // Avoid duplicate emails for married couples with a shared email address
             .GroupBy(p => p.Email)
             .Select(grouping => new MailboxAddress(
                 name: string.Join(", ", grouping.Select(r => r.FirstName)) + ' ' + grouping.First().LastName,
                 address: grouping.Key))
             .ToArray();
    }

    public async ValueTask<ISet<Person>> GetPeopleRecursive(PersonFilter personFilter, CancellationToken cancellationToken)
    {
        HashSet<Person> people = new();

        if (personFilter is LogicalOr)
        {
            List<PersonFilter> children = await database.PersonFilters
                .Where(filter => filter.ParentId == personFilter.Id)
                .ToListAsync(cancellationToken);

            foreach (PersonFilter child in children)
            {
                people.UnionWith(await GetPeopleRecursive(child, cancellationToken));
            }
        }
        else if (personFilter is LogicalAnd)
        {
            List<PersonFilter> children = await database.PersonFilters
                .Where(filter => filter.ParentId == personFilter.Id)
                .ToListAsync(cancellationToken);

            foreach (PersonFilter child in children)
            {
                people.IntersectWith(await GetPeopleRecursive(child, cancellationToken));
            }
        }
        else if (personFilter is GroupFilter groupFilter)
        {
            people.UnionWith(
                await database.GroupMembers
                    .Where(m => m.GroupId == groupFilter.GroupId && (groupFilter.GroupRoleId == null || m.GroupRoleId == groupFilter.GroupRoleId))
                    .Join(database.People, m => m.PersonId, p => p.Id, (m, p) => p)
                    .ToListAsync(cancellationToken));
        }
        else if (personFilter is StatusFilter statusFilter)
        {
            people.UnionWith(
                await database.People
                    .Where(p => p.StatusId == statusFilter.StatusId && !string.IsNullOrEmpty(p.Email))
                    .ToListAsync(cancellationToken));
        }
        else if (personFilter is SinglePerson singlePerson)
        {
            people.Add(
                await database.People.SingleAsync(p => p.Id == singlePerson.PersonId, cancellationToken));
        }

        return people;
    }
}
