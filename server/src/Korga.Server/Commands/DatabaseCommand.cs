using Korga.Server.Database;
using Korga.Server.Database.Entities;
using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CA1822 // Mark members as static
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
            // Admin
            var admin = new Person("Karl-Heinz", "Günther") { MailAddress = "gunther@example.com" };
            database.People.Add(admin);
            await database.SaveChangesAsync();

            // Members of multiple groups/roles
            var max = new Person("Max", "Mustermann") { MailAddress = "mustermann@example.com", CreatedById = admin.Id };
            var linda = new Person("Linda", "Koch") { MailAddress = "lindakoch@example.com", CreatedById = admin.Id };
            database.People.AddRange(max, linda);

            // Official members
            List<Person> members = new()
            {
                new("Susanne", "Günther") { MailAddress = "susanne.gunther@example.com", CreatedById = admin.Id },
                new("David", "Neumann") { MailAddress = "neumann@example.com", CreatedById = admin.Id },
                new("Eva", "Neumann") { MailAddress = "neumann@example.com", CreatedById = admin.Id },
                new("Johannes", "Schäfer") { MailAddress = "j.schaefer@example.com", CreatedById = admin.Id },
                new("Katharina", "Schäfer") { MailAddress = "k.schaefer@example.com", CreatedById = admin.Id }
            };
            database.People.AddRange(members);
            members.AddRange(new[] { admin, max, linda});

            // Youth members
            List<Person> youths = new()
            {
                new("Elias", "Müller") { CreatedById = admin.Id },
                new("Leonie", "Neumann") { CreatedById = admin.Id },
                new("Jonas", "Neumann") { CreatedById = admin.Id },
                new("Sarah", "Schulz") { CreatedById = admin.Id },
                new("Anna", "Schäfer") { CreatedById = admin.Id },
                new("Lukas", "Meyer") { CreatedById = admin.Id }
            };
            database.People.AddRange(youths);
            youths.AddRange(new[] { max, linda });

            var memberGroup = new Group("Mitglieder") { Description = "Mitglieder der Gemeinde", CreatedById = admin.Id };
            var youthGroup = new Group("Jugend") { Description = "Gruppe für Jugendliche ab 14 Jahren", CreatedById = admin.Id };
            database.Groups.AddRange(memberGroup, youthGroup);
            await database.SaveChangesAsync();

            var memberMember = new GroupRole("Mitglied") { GroupId = memberGroup.Id, CreatedById = admin.Id };
            var youthMember = new GroupRole("Teilnehmer") { GroupId = youthGroup.Id, CreatedById = admin.Id };
            var youthLeader = new GroupRole("Leiter") { GroupId = youthGroup.Id, CreatedById = admin.Id };
            database.GroupRoles.AddRange(memberMember, youthMember, youthLeader);
            await database.SaveChangesAsync();

            database.GroupMembers.AddRange(
                members.Select(m => new GroupMember { PersonId = m.Id, GroupRoleId = memberMember.Id, CreatedById = admin.Id }));

            database.GroupMembers.Add(new GroupMember { PersonId = max.Id, GroupRoleId = youthLeader.Id, CreatedById = admin.Id });
            database.GroupMembers.AddRange(
                youths.Select(y => new GroupMember { PersonId = y.Id, GroupRoleId = youthMember.Id, CreatedById = admin.Id }));

            await database.SaveChangesAsync();
        }
    }
}
