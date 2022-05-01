using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Korga.Server.Services;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests.EventController;

public class QueryParticipantsTests : SQLiteIntegrationTest
{
    [Fact]
    public async Task TestCaseInsesitivity()
    {
        var sampleDataService = GetRequiredService<EventSampleDataService>();
        var registrationService = GetRequiredService<EventRegistrationService>();

        Event @event = await sampleDataService.CreateDefaultService(10, 0);
        EventRegistration registration =
            await registrationService.CreateRegistration(@event.Id, new EventRegistrationRequest[]
            {
                new("Max", "Mustermann") { ProgramId = @event.Programs![0].EventId }
            });

        string url = $"/api/event/{@event.Id}"; // /participants/query"; //?givenName=&familyName=";

        var response = await TestClient.GetFromJsonAsync<EventParticipantQueryResponse>(url);
    }
}
