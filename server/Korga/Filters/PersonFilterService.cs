using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Korga.ChurchTools.Entities;
using Korga.Extensions;
using Korga.Filters.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korga.Filters;

public class PersonFilterService
{
    private readonly DatabaseContext database;
    private readonly PersonLookupService lookupService;

    public PersonFilterService(DatabaseContext database, PersonLookupService lookupService)
    {
        this.database = database;
        this.lookupService = lookupService;
    }

    /// <summary>
    /// Returns a collection of all people that match at least one of the filters of the given list.
    /// </summary>
    /// <param name="filterListId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An unordered collection of distinct people that are matched by the filter list.</returns>
    public async ValueTask<IEnumerable<Person>> GetPeople(long filterListId, CancellationToken cancellationToken = default)
    {
        List<PersonFilter> personFilters = await database.PersonFilters
            .Where(f => f.PersonFilterListId == filterListId)
            .ToListAsync(cancellationToken);

        if (personFilters.Count == 0) return [];

        IQueryable<Person> query = FilterToQuery(personFilters[0]);

        for (int i = 1; i < personFilters.Count; i++)
        {
            query = query.Union(FilterToQuery(personFilters[i]));
        }

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async ValueTask<bool> HasPermission(ClaimsPrincipal user, string permissionKey)
    {
        Person? person = await lookupService.GetPerson(user);
        if (person == null) return false;

        List<PersonFilter> personFilters = await database.Permissions.Where(p => p.Key == permissionKey)
            .Join(database.PersonFilters, p => p.PersonFilterListId, f => f.PersonFilterListId, (p, f) => f)
            .ToListAsync();

        foreach (PersonFilter filter in personFilters)
        {
            if (await FilterToQuery(filter).AnyAsync(p => p.Id == person.Id)) return true;
        }

        return false;
    }

    private IQueryable<Person> FilterToQuery(PersonFilter filter)
    {
        if (filter is GroupFilter groupFilter)
        {
            return database.GroupMembers
                .Where(m => m.GroupId == groupFilter.GroupId && (groupFilter.GroupRoleId == null || m.GroupRoleId == groupFilter.GroupRoleId))
                .Join(database.People, m => m.PersonId, p => p.Id, (m, p) => p);
        }
        else if (filter is GroupTypeFilter groupTypeFilter)
        {
            return database.Groups
                .Where(g => g.GroupTypeId == groupTypeFilter.GroupTypeId)
                .Join(database.GroupMembers, g => g.Id, m => m.GroupId, (g, m) => m)
                .Where(m => groupTypeFilter.GroupRoleId == null || m.GroupRoleId == groupTypeFilter.GroupRoleId)
                .Join(database.People, m => m.PersonId, p => p.Id, (m, p) => p);
        }
        else if (filter is StatusFilter statusFilter)
        {
            return database.People.Where(p => p.StatusId == statusFilter.StatusId);
        }
        else if (filter is SinglePerson singlePerson)
        {
            return database.People.Where(p => p.Id == singlePerson.PersonId);
        }

        throw new ArgumentException($"Invalid filter type {filter.GetType().FullName}", nameof(filter));
    }

    public async ValueTask<bool> AddFilter(long filterListId, PersonFilter filter)
    {
        try
        {
            filter.PersonFilterListId = filterListId;
            database.PersonFilters.Add(filter);
            await database.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return false;
        }
    }
}
