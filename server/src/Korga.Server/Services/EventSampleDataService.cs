using Korga.Server.Database;
using Korga.Server.Database.Entities;
using System;
using System.Threading.Tasks;

namespace Korga.Server.Services
{
    public class EventSampleDataService
    {
        private readonly TimeZoneInfo timeZone;
        private readonly DatabaseContext database;

        public EventSampleDataService(DatabaseContext database)
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            this.database = database;
        }

        public async Task<Event> CreateDefaultService(int hour, int minute)
        {
            Event service = new("Gottesdienst")
            {
                Start = NextSundayAt(10, 0),
                End = NextSundayAt(11, 30),
                RegistrationStart = NextSundayAt(0, 0).AddDays(-7),
                RegistrationDeadline = NextSundayAt(10, 0),
                Programs = new[] { new EventProgram("Gottesdienst") { Limit = 50 } }
            };
            database.Events.Add(service);
            await database.SaveChangesAsync();
            return service;
        }

        private DateTime NextSundayAt(int hour, int minute)
        {
            DateTime nowWesternEurope = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            DateTime timeAtNextSundayWesternEurope = nowWesternEurope.Date
                .AddDays(7 - (int)DateTime.UtcNow.DayOfWeek)
                .AddHours(hour)
                .AddMinutes(minute);

            return TimeZoneInfo.ConvertTimeToUtc(timeAtNextSundayWesternEurope, timeZone);
        }
    }
}
