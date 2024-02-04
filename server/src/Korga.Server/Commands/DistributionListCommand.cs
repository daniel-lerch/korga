using Korga.EmailRelay.Entities;
using Korga.Server.EmailRelay;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands;

[Command("dist")]
[Subcommand(typeof(Create), typeof(AddRecipient), typeof(List))]
public class DistributionListCommand
{
    private int OnExecute(CommandLineApplication app)
    {
        app.ShowHint();
        return 1;
    }

    [Command("create")]
    public class Create
    {
        [Argument(0)] public string? Alias { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database)
        {
            if (!string.IsNullOrWhiteSpace(Alias))
            {
                database.DistributionLists.Add(new(Alias));
                await database.SaveChangesAsync();

                return 0;
            }
            else
            {
                console.Out.WriteLine("Invalid alias");
                return 1;
            }
        }
    }

    [Command("add-recipient")]
    public class AddRecipient
    {
        [Argument(0)] public string? Alias { get; set; }
        [Option] public int? StatusId { get; set; }
        [Option] public int? GroupId { get; set; }
        [Option("--group-type-id")] public int? GroupTypeId { get; set; }
        [Option("--group-role-id")] public int? GroupRoleId { get; set; }
        [Option] public int? PersonId { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database, DistributionListService distributionListService)
        {
            DistributionList? distributionList = await database.DistributionLists
                .Include(dl => dl.PermittedRecipients)
                .SingleOrDefaultAsync(dl => dl.Alias == Alias);

            if (distributionList == null)
            {
                console.Out.WriteLine("Distribution list {0} not found", Alias);
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

            database.PersonFilters.Add(additionalFilter);
            distributionList.PermittedRecipients = distributionListService.AddPersonFilter(distributionList.PermittedRecipients, additionalFilter);
            await database.SaveChangesAsync();
            return 0;
        }
    }

    [Command("list")]
    public class List
    {
        private async Task OnExecute(IConsole console, DatabaseContext database)
        {
            List<DistributionList> distributionLists = await database.DistributionLists.Include(dl => dl.PermittedRecipients).ToListAsync();
            foreach (DistributionList distributionList in distributionLists)
            {
                console.Out.WriteLine("#{0} Alias: {1}", distributionList.Id, distributionList.Alias);
                console.Out.WriteLine("Recipients:");

                async Task printFiltersRecursive(PersonFilter filter, int indentationLevel)
                {
                    console.Out.Write(new string(' ', 2 * indentationLevel));

                    if (filter is LogicalOr)
                    {
                        console.Out.WriteLine("- OR");

                        foreach (PersonFilter child in await database.PersonFilters.Where(child => child.ParentId == filter.Id).ToListAsync())
                        {
                            await printFiltersRecursive(child, indentationLevel + 1);
                        }
                    }
                    else if (filter is LogicalAnd)
                    {
                        console.Out.WriteLine("- AND");

                        foreach (PersonFilter child in await database.PersonFilters.Where(child => child.ParentId == filter.Id).ToListAsync())
                        {
                            await printFiltersRecursive(child, indentationLevel + 1);
                        }
                    }
                    else if (filter is GroupFilter groupFilter)
                    {
                        if (groupFilter.GroupRoleId.HasValue)
                            console.Out.WriteLine("- Group Filter Id: {0} Role: {1}", groupFilter.GroupId, groupFilter.GroupRoleId);
                        else
                            console.Out.WriteLine("- Group Filter Id: {0}", groupFilter.GroupId);
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

                if (distributionList.PermittedRecipients != null)
                    await printFiltersRecursive(distributionList.PermittedRecipients, 0);

                console.Out.WriteLine();
            }
        }
    }
}
