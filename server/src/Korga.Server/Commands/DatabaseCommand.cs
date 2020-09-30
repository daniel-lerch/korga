using Korga.Server.Database;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands
{
    [Command("database")]
    [Subcommand(typeof(Create), typeof(Delete))]
    public class DatabaseCommand
    {
        private static async Task DeleteDatabase(CommandLineApplication app, DatabaseContext database)
        {
            if (Prompt.GetYesNo("Do you really want to delete the Korga database?", false))
            {
                bool deleted = await database.Database.EnsureDeletedAsync();
                if (!deleted) app.Error.WriteLine("Notice: No database found to delete.");
            }
        }

        private int OnExecute(CommandLineApplication app)
        {
            app.ShowHint();
            return 1;
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
    }
}
