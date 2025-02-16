﻿using Korga.Filters;
using Korga.Filters.Entities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Commands;

[Command("permission")]
[Subcommand(typeof(Grant), typeof(Withdraw), typeof(List))]
public class PermissionCommand
{
    private int OnExecute(CommandLineApplication app)
    {
        app.ShowHint();
        return 1;
    }


    [Command("grant")]
    public class Grant
    {
        [Argument(0)] public Permissions? PermissionName { get; set; }
        [Option] public int? StatusId { get; set; }
        [Option] public int? GroupId { get; set; }
        [Option("--group-type-id")] public int? GroupTypeId { get; set; }
        [Option("--group-role-id")] public int? GroupRoleId { get; set; }
        [Option] public int? PersonId { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database, PersonFilterService filterService)
        {
            Permission? permission = await database.Permissions.SingleOrDefaultAsync(p => p.Key == PermissionName);

            if (permission == null)
            {
                console.Out.WriteLine("Permission {0} not found", PermissionName);
                return 1;
            }

            PersonFilter? additionalFilter = null;

            if (StatusId == null && GroupId == null && GroupTypeId == null && PersonId == null)
            {
                console.Out.WriteLine("You must specify one of Status ID, Group ID, Group Type ID and Person ID");
                return 1;
            }
            else if (StatusId.HasValue && GroupId == null && GroupTypeId == null && PersonId == null)
            {
                additionalFilter = new StatusFilter() { StatusId = StatusId.Value };
            }
            else if (StatusId == null && GroupId.HasValue && GroupTypeId == null && PersonId == null)
            {
                additionalFilter = new GroupFilter() { GroupId = GroupId.Value, GroupRoleId = GroupRoleId };
            }
            else if (StatusId == null && GroupId == null && GroupTypeId.HasValue && PersonId == null)
            {
                additionalFilter = new GroupTypeFilter() { GroupTypeId = GroupTypeId.Value, GroupRoleId = GroupRoleId };
            }
            else if (StatusId == null && GroupId == null && GroupTypeId == null && PersonId.HasValue)
            {
                additionalFilter = new SinglePerson() { PersonId = PersonId.Value };
            }
            else
            {
                console.Out.WriteLine("Status ID, Group ID, Group Type ID and Person ID are mutually exclusive");
                return 1;
            }

            if (!await filterService.AddFilter(permission.PersonFilterListId, additionalFilter))
                console.Out.WriteLine("Filter already exists");

            return 0;
        }
    }

    [Command("withdraw")]
    public class Withdraw
    {
        [Argument(0)] public Permissions? PermissionName { get; set; }
        [Option] public int? StatusId { get; set; }
        [Option] public int? GroupId { get; set; }
        [Option("--group-type-id")] public int? GroupTypeId { get; set; }
        [Option("--group-role-id")] public int? GroupRoleId { get; set; }
        [Option] public int? PersonId { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database, PersonFilterService filterService)
        {
            Permission? permission = await database.Permissions.SingleOrDefaultAsync(p => p.Key == PermissionName);

            if (permission == null)
            {
                console.Out.WriteLine("Permission {0} not found", PermissionName);
                return 1;
            }

            IQueryable<PersonFilter> filters = database.PersonFilters
                .Where(f => f.PersonFilterListId == permission.PersonFilterListId);

            PersonFilter? filterToDelete;

            if (StatusId == null && GroupId == null && GroupTypeId == null && PersonId == null)
            {
                console.Out.WriteLine("You must specify one of Status ID, Group ID, Group Type ID and Person ID");
                return 1;
            }
            else if (StatusId.HasValue && GroupId == null && GroupTypeId == null && PersonId == null)
            {
                filterToDelete = await filters.OfType<StatusFilter>()
                    .SingleOrDefaultAsync(sf => sf.StatusId == StatusId.Value);
            }
            else if (StatusId == null && GroupId.HasValue && GroupTypeId == null && PersonId == null)
            {
                filterToDelete = await filters.OfType<GroupFilter>()
                    .SingleOrDefaultAsync(gf => gf.GroupId == GroupId.Value && gf.GroupRoleId == GroupRoleId);
            }
            else if (StatusId == null && GroupId == null && GroupTypeId.HasValue && PersonId == null)
            {
                filterToDelete = await filters.OfType<GroupTypeFilter>()
                    .SingleOrDefaultAsync(gtf => gtf.GroupTypeId == GroupTypeId.Value && gtf.GroupRoleId == GroupRoleId);
            }
            else if (StatusId == null && GroupId == null && GroupTypeId == null && PersonId.HasValue)
            {
                filterToDelete = await filters.OfType<SinglePerson>()
                    .SingleOrDefaultAsync(sp => sp.PersonId == PersonId.Value);
            }
            else
            {
                console.Out.WriteLine("Status ID, Group ID, Group Type ID and Person ID are mutually exclusive");
                return 1;
            }

            if (filterToDelete == null)
            {
                console.Out.WriteLine("Filter not found");
                return 1;
            }

            database.PersonFilters.Remove(filterToDelete);
            await database.SaveChangesAsync();
            return 0;
        }
    }

    [Command("list")]
    public class List
    {
        private async Task OnExecute(IConsole console, DatabaseContext database)
        {
            List<Permission> permissions = await database.Permissions
                .Include(dl => dl.PersonFilterList)
                .ThenInclude(fl => fl!.Filters)
                .ToListAsync();

            foreach (Permission permission in permissions)
            {
                console.Out.WriteLine(permission.Key.ToString());

                if (permission.PersonFilterList == null || permission.PersonFilterList.Filters == null)
                    continue;

                foreach (PersonFilter filter in permission.PersonFilterList.Filters)
                {
                    if (filter is GroupFilter groupFilter)
                    {
                        if (groupFilter.GroupRoleId.HasValue)
                            console.Out.WriteLine("- Group Filter Id: {0} Role: {1}", groupFilter.GroupId, groupFilter.GroupRoleId);
                        else
                            console.Out.WriteLine("- Group Filter Id: {0}", groupFilter.GroupId);
                    }
                    else if (filter is GroupTypeFilter groupTypeFilter)
                    {
                        if (groupTypeFilter.GroupRoleId.HasValue)
                            console.Out.WriteLine("- Group Type Filter Id: {0} Role: {1}", groupTypeFilter.GroupTypeId, groupTypeFilter.GroupRoleId);
                        else
                            console.Out.WriteLine("- Group Type Filter Id: {0}", groupTypeFilter.GroupTypeId);
                    }
                    else if (filter is StatusFilter statusFilter)
                    {
                        console.Out.WriteLine("- Status Filter Id: {0}", statusFilter.StatusId);
                    }
                    else if (filter is SinglePerson singlePerson)
                    {
                        console.Out.WriteLine("- Single Person Id: {0}", singlePerson.PersonId);
                    }
                }

                console.Out.WriteLine();
            }
        }
    }
}
