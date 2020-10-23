using Korga.Server.Database;
using Korga.Server.Database.Entities;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands
{
    [Command("database")]
    [Subcommand(typeof(Create), typeof(Delete), typeof(Populate))]
    public class DatabaseCommand
    {
        private const string populateDescription = "Fills an existing database with example data for testing.";

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

            [Option(Description = populateDescription)]
            public bool Populate { get; set; }

            private async Task OnExecute(CommandLineApplication app, DatabaseContext database)
            {
                if (Force) await DeleteDatabase(app, database);

                await database.Database.EnsureCreatedAsync();

                if (Populate) await PopulateDatabase(database);
            }
        }

        [Command("delete")]
        public class Delete
        {
            private Task OnExecute(CommandLineApplication app, DatabaseContext database) => DeleteDatabase(app, database);
        }

        [Command("populate", Description = populateDescription)]
        public class Populate
        {
            private Task OnExecute(DatabaseContext database) => PopulateDatabase(database);
        }

        private static async Task DeleteDatabase(CommandLineApplication app, DatabaseContext database)
        {
            if (Prompt.GetYesNo("Do you really want to delete the Korga database?", false))
            {
                bool deleted = await database.Database.EnsureDeletedAsync();
                if (!deleted) app.Error.WriteLine("Notice: No database found to delete.");
            }
        }

        private static async Task PopulateDatabase(DatabaseContext database)
        {
            var admin = new Person("Karl-Heinz", "Günther") { MailAddress = "gunther@example.com" };
            database.People.Add(admin);
            await database.SaveChangesAsync();

            var person = new Person("Max", "Mustermann") { MailAddress = "mustermann@example.com", CreatorId = admin.Id };
            database.People.Add(person);

            var group = new Group("Jugend") { Description = "Gruppe für Jugendliche ab 14 Jahren", CreatorId = admin.Id };
            database.Groups.Add(group);
            await database.SaveChangesAsync();

            var member = new GroupRole("Teilnehmer") { GroupId = group.Id, CreatorId = admin.Id };
            var leader = new GroupRole("Leiter") { GroupId = group.Id, CreatorId = admin.Id };
            database.GroupRoles.Add(member);
            database.GroupRoles.Add(leader);
            await database.SaveChangesAsync();

            database.GroupMembers.Add(new GroupMember { PersonId = person.Id, GroupRoleId = member.Id, CreatorId = admin.Id });
            database.GroupMembers.Add(new GroupMember { PersonId = admin.Id, GroupRoleId = member.Id, CreatorId = admin.Id });
            database.GroupMembers.Add(new GroupMember { PersonId = admin.Id, GroupRoleId = leader.Id, CreatorId = admin.Id });
            await database.SaveChangesAsync();
        }
    }
}
