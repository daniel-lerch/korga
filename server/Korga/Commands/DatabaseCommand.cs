using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Commands;

[Command("database")]
[Subcommand(typeof(Migrate), typeof(Create), typeof(Delete))]
public class DatabaseCommand
{
    private const string populateDescription = "Fills an existing database with example data for testing.";

    private int OnExecute(CommandLineApplication app)
    {
        app.ShowHint();
        return 1;
    }

    [Command("migrate")]
    public class Migrate
    {
        private async Task OnExecute(DatabaseContext database)
        {
            await database.Database.MigrateAsync();
        }
    }

    [Command("create")]
    public class Create
    {
        [Option(Description = "Forces a recreation of the database")]
        public bool Force { get; set; }

        private async Task OnExecute(CommandLineApplication app, DatabaseContext database)
        {
            if (Force) await DeleteDatabase(app, database);

            await database.Database.EnsureCreatedAsync();
        }
    }

    [Command("delete")]
    public class Delete
    {
        private Task OnExecute(CommandLineApplication app, DatabaseContext database) => DeleteDatabase(app, database);
    }

    private static async Task DeleteDatabase(CommandLineApplication app, DatabaseContext database)
    {
        if (Prompt.GetYesNo("Do you really want to delete the Korga database?", false))
        {
            bool deleted = await database.Database.EnsureDeletedAsync();
            if (!deleted) app.Error.WriteLine("Notice: No database found to delete.");
        }
    }
}
