using Korga.EmailRelay.Entities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Commands;

[Command("dist")]
[Subcommand(typeof(Create), typeof(List), typeof(Remove))]
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
        [Option] public string? RecipientsQuery { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database)
        {
            if (!string.IsNullOrWhiteSpace(Alias))
            {
                database.DistributionLists.Add(new(Alias) { RecipientsQuery = RecipientsQuery });
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
            List<DistributionList> distributionLists = await database.DistributionLists.ToListAsync();

            foreach (DistributionList distributionList in distributionLists)
            {
                console.Out.WriteLine("#{0} Alias: {1}", distributionList.Id, distributionList.Alias);

                if (string.IsNullOrEmpty(distributionList.RecipientsQuery))
                    continue;

                console.Out.WriteLine("    Recipients Query: {0}", distributionList.RecipientsQuery);

                console.Out.WriteLine();
            }
        }
    }

    [Command("remove")]
    public class Remove
    {
        [Argument(0)] public string? Identifier { get; set; }

        private async Task<int> OnExecute(IConsole console, DatabaseContext database)
        {
            if (string.IsNullOrWhiteSpace(Identifier))
            {
                console.Out.WriteLine("Missing identifier (id or alias)");
                return 1;
            }

            DistributionList? distributionList = null;

            if (long.TryParse(Identifier, out var id))
            {
                distributionList = await database.DistributionLists.FindAsync(id);
            }

            if (distributionList == null)
            {
                distributionList = await database.DistributionLists.FirstOrDefaultAsync(d => d.Alias == Identifier);
            }

            if (distributionList == null)
            {
                console.Out.WriteLine("Distribution list not found");
                return 1;
            }

            database.DistributionLists.Remove(distributionList);
            await database.SaveChangesAsync();

            console.Out.WriteLine("Deleted distribution list #{0} Alias: {1}", distributionList.Id, distributionList.Alias);
            return 0;
        }
    }
}
