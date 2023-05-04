using Korga.EmailRelay.Entities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands;

[Command("distribution-list")]
[Subcommand(typeof(Create), typeof(List))]
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
        [Option] public int? StatusId { get; set; }
        [Option] public int? GroupId { get; set; }
        [Option("--group-role-id")] public int? GroupRoleId { get; set; }
        [Option] public int? PersonId { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database)
        {
            if (!string.IsNullOrWhiteSpace(Alias))
            {
                PersonFilter? filter;
                if (StatusId == null && GroupId == null && PersonId == null)
                    filter = null;
                else if (StatusId.HasValue && GroupId == null && PersonId == null)
                    filter = new StatusFilter() { StatusId = StatusId.Value };
                else if (StatusId == null && GroupId.HasValue && PersonId == null)
                    filter = new GroupFilter() { GroupId = GroupId.Value, GroupRoleId = GroupRoleId };
                else if (StatusId == null && GroupId == null && PersonId.HasValue)
                    filter = new SinglePerson() { PersonId = PersonId.Value };
                else
                {
                    console.Out.WriteLine("Status ID, Group ID and Person ID are mutually exclusive");
                    return 1;
                }

                DistributionList distributionList = new(Alias) { PermittedRecipients = filter };
                database.DistributionLists.Add(distributionList);
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

    [Command("list")]
    public class List
    {
        private async Task OnExecute(IConsole console, DatabaseContext database)
        {
            List<DistributionList> distributionLists = await database.DistributionLists.Include(dl => dl.PermittedRecipients).ToListAsync();
            foreach (DistributionList distributionList in distributionLists)
            {
                console.Out.WriteLine("#{0} Alias: {1}", distributionList.Id, distributionList.Alias);

                async Task printFiltersRecursive(PersonFilter filter)
                {
                    if (filter is LogicalOr)
                    {
                        console.Out.WriteLine("-- Begin OR");

                        foreach (PersonFilter child in await database.PersonFilters.Where(filter => filter.ParentId == filter.Id).ToListAsync())
                        {
                            await printFiltersRecursive(child);
                        }

                        console.Out.WriteLine("-- End OR");
                    }
                    else if (filter is LogicalAnd)
                    {
                        console.Out.WriteLine("-- Begin AND");

                        foreach (PersonFilter child in await database.PersonFilters.Where(filter => filter.ParentId == filter.Id).ToListAsync())
                        {
                            await printFiltersRecursive(child);
                        }

                        console.Out.WriteLine("-- End AND");
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

                if (distributionList.PermittedSenders != null)
                    await printFiltersRecursive(distributionList.PermittedSenders);

                console.Out.WriteLine();
            }
        }
    }
}
