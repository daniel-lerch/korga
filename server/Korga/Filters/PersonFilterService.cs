using Korga.ChurchTools.Entities;
using Korga.Extensions;
using Korga.Filters.Entities;
using Korga.Models.Json;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

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

    public async ValueTask<bool> HasPermission(ClaimsPrincipal user, Permissions permission)
    {
        Person? person = await lookupService.GetPerson(user);
        if (person == null) return false;

        List<PersonFilter> personFilters = await database.Permissions.Where(p => p.Key == permission)
            .Join(database.PersonFilters, p => p.PersonFilterListId, f => f.PersonFilterListId, (p, f) => f)
            .ToListAsync();

        foreach (PersonFilter filter in personFilters)
        {
            if (await FilterToQuery(filter).AnyAsync(p => p.Id == person.Id)) return true;
        }

        return false;
    }

    public IQueryable<Person> FilterToQuery(PersonFilter filter)
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

    public async ValueTask<PersonFilterResponse> GetFilterResponse(PersonFilter personFilter)
    {
        PersonFilterResponse filter = new() { Id = personFilter.Id, Discriminator = personFilter.GetType().Name };

        if (personFilter is StatusFilter statusFilter)
        {
            filter.StatusName = await database.Status.Where(s => s.Id == statusFilter.StatusId).Select(s => s.Name).SingleAsync();
        }
        else if (personFilter is GroupFilter groupFilter)
        {
            filter.GroupName = await database.Groups.Where(g => g.Id == groupFilter.GroupId).Select(g => g.Name).SingleAsync();
            if (groupFilter.GroupRoleId.HasValue)
                filter.GroupRoleName = await database.GroupRoles.Where(r => r.Id == groupFilter.GroupRoleId.Value).Select(r => r.Name).SingleAsync();
        }
        else if (personFilter is GroupTypeFilter groupTypeFilter)
        {
            filter.GroupTypeName = await database.GroupTypes.Where(t => t.Id == groupTypeFilter.GroupTypeId).Select(t => t.Name).SingleAsync();
            if (groupTypeFilter.GroupRoleId.HasValue)
                filter.GroupRoleName = await database.GroupRoles.Where(r => r.Id == groupTypeFilter.GroupRoleId.Value).Select(r => r.Name).SingleAsync();
        }
        else if (personFilter is SinglePerson singlePerson)
        {
            filter.PersonFullName = await database.People.Where(p => p.Id == singlePerson.PersonId).Select(p => $"{p.FirstName} {p.LastName}").SingleAsync();
        }
        else
        {
            throw new ArgumentException($"Invalid filter type {personFilter.GetType().FullName}", nameof(personFilter));
        }

        return filter;
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
