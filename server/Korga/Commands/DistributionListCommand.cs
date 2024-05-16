using Korga.EmailRelay.Entities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Commands;

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
				DistributionList distributionList = new(Alias);
				database.DistributionLists.Add(distributionList);
				await database.SaveChangesAsync();

				if (StatusId.HasValue)
				{
					database.PersonFilters.Add(new StatusFilter() { DistributionListId = distributionList.Id, StatusId = StatusId.Value });
					await database.SaveChangesAsync();
				}

				if (GroupId.HasValue)
				{
					database.PersonFilters.Add(new GroupFilter() { DistributionListId = distributionList.Id, GroupId = GroupId.Value, GroupRoleId = GroupRoleId });
					await database.SaveChangesAsync();
				}

				if (PersonId.HasValue)
				{
					database.PersonFilters.Add(new SinglePerson() { DistributionListId = distributionList.Id, PersonId = PersonId.Value });
					await database.SaveChangesAsync();
				}

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
			List<DistributionList> distributionLists = await database.DistributionLists.Include(dl => dl.Filters).ToListAsync();
			foreach (DistributionList distributionList in distributionLists)
			{
				console.Out.WriteLine("#{0} Alias: {1}", distributionList.Id, distributionList.Alias);

				foreach (PersonFilter filter in distributionList.Filters!)
				{
					if (filter is GroupFilter groupFilter)
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

				console.Out.WriteLine();
			}
		}
	}
}
