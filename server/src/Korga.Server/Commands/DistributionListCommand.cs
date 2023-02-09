using Korga.EmailRelay.Entities;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands;

[Command("distribution-list")]
[Subcommand(typeof(Create))]
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

				return 0;
			}
			else
			{
				console.Out.WriteLine("Invalid alias");
				return 1;
			}
		}
	}
}
