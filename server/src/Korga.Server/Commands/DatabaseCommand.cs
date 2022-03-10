using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Models;
using McMaster.Extensions.CommandLineUtils;
using System;
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
            var nextSunday = DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek);
            DateTime time(int hour, int minute) => nextSunday.AddHours(hour - 1).AddMinutes(minute);

            // Create two services as events
            var service10 = new Event("Gottesdienst")
            {
                Start = time(10, 0),
                End = time(11, 30),
                RegistrationPeriod = RegistrationPeriod.OpenWithFirst
            };
            var service12 = new Event("Gottesdienst")
            {
                Start = time(12, 0),
                End = time(13, 30)
            };
            database.Events.AddRange(service10, service12);
            await database.SaveChangesAsync();

            // Create children's ministry as programs
            var earlyOpen = nextSunday.AddDays(-6).AddHours(-1);
            var lateOpen = nextSunday.AddDays(-5).AddHours(20 - 1);

            var program10_0 = new EventProgram("Gottesdienst")
            {
                EventId = service10.Id,
                RegistrationStart = lateOpen,
                RegistrationDeadline = service10.Start,
                Limit = 65
            };
            var program10_1 = new EventProgram("Kükennest (0-3 Jahre)")
            {
                EventId = service10.Id,
                RegistrationStart = earlyOpen,
                RegistrationDeadline = service10.Start,
                Limit = 5
            };
            var program10_2 = new EventProgram("Kindergartenkinder")
            {
                EventId = service10.Id,
                RegistrationStart = earlyOpen,
                RegistrationDeadline = service10.Start,
                Limit = 12
            };
            var program10_3 = new EventProgram("Grundschulkinder")
            {
                EventId = service10.Id,
                RegistrationStart = earlyOpen,
                RegistrationDeadline = service10.Start,
                Limit = 12
            };
            var program10_4 = new EventProgram("Weiterführende Schule")
            {
                EventId = service10.Id,
                RegistrationStart = earlyOpen,
                RegistrationDeadline = service10.Start,
                Limit = 12
            };

            var program12_0 = new EventProgram("Gottesdienst")
            {
                EventId = service12.Id,
                RegistrationStart = earlyOpen,
                RegistrationDeadline = service12.Start,
                Limit = 65
            };

            database.EventPrograms.AddRange(program10_0, program10_1, program10_2, program10_3, program10_4, program12_0);
            await database.SaveChangesAsync();
        }
    }
}
