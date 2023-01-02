using Korga.Server.Database;
using Korga.Server.Database.EventRegistration;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests.Http;

public class EventControllerTests : IDisposable
{
    private readonly TestServer server;
    private readonly HttpClient client;
    private readonly IServiceScope serviceScope;
    private readonly DatabaseContext database;

    private readonly Event testEvent;
    private readonly EventProgram testProgram;

    public EventControllerTests()
    {
        server = TestHost.CreateTestServer();
        client = server.CreateClient();
        serviceScope = server.Services.CreateScope();
        database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

        testEvent = new Event(Guid.NewGuid().ToString());
        database.Events.Add(testEvent);
        database.SaveChanges();

        testProgram = new EventProgram(Guid.NewGuid().ToString())
        {
            EventId = testEvent.Id,
            Limit = 2
        };
        database.EventPrograms.Add(testProgram);
        database.SaveChanges();
    }

    public void Dispose()
    {
        database.Events.Remove(testEvent);
        database.SaveChanges();
        serviceScope.Dispose();
        server.Dispose();
        client.Dispose();
    }

    [Fact]
    public async Task TestGetEvents_Empty()
    {
        var events = await client.GetFromJsonAsync<EventResponse[]>("/api/events");
        Assert.NotNull(events);
        var response = events.Single(e => e.Id == testEvent.Id);
        Assert.Equal(testEvent.Name, response.Name);
        var program = response.Programs.Single();
        Assert.Equal(testProgram.Id, program.Id);
        Assert.Equal(testProgram.Name, program.Name);
        Assert.Equal(testProgram.Limit, program.Limit);
        Assert.Equal(0, program.Count);
    }

    [Fact]
    public async Task TestGetEvent_Full()
    {
        var participants = new EventParticipant[]
        {
            new("Katharina", "Schäfer") { ProgramId = testProgram.Id },
            new("Anna", "Schäfer") { ProgramId = testProgram.Id }
        };
        database.EventParticipants.AddRange(participants);
        await database.SaveChangesAsync();

        var response = await client.GetFromJsonAsync<EventResponse2>($"/api/event/{testEvent.Id}");
        Assert.NotNull(response);
        Assert.Equal(testEvent.Name, response.Name);
        var program = response.Programs.Single();
        Assert.Equal(testProgram.Id, program.Id);
        Assert.Equal(testProgram.Name, program.Name);
        Assert.Equal(testProgram.Limit, program.Limit);
        Assert.Equal(2, program.Participants.Count);
        Assert.NotEqual(0, program.Participants.Single(p => p.GivenName == "Katharina" && p.FamilyName == "Schäfer").Id);
        Assert.NotEqual(0, program.Participants.Single(p => p.GivenName == "Anna" && p.FamilyName == "Schäfer").Id);
    }

    [Fact]
    public async Task TestRegister_Single()
    {
        var request = new EventRegistrationRequest("Max", "Mustermann") { ProgramId = testProgram.Id };
        var response = await client.PostAsJsonAsync("/api/events/register", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task TestRegister_TooMany()
    {
        EventRegistrationRequest request1 = new("Johannes", "Schäfer") { ProgramId = testProgram.Id };
        var response1 = await client.PostAsJsonAsync("/api/events/register", request1);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

        EventRegistrationRequest request2 = new("Katharina", "Schäfer") { ProgramId = testProgram.Id };
        var response2 = await client.PostAsJsonAsync("/api/events/register", request2);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);

        EventRegistrationRequest request3 = new("Anna", "Schäfer") { ProgramId = testProgram.Id };
        var response3 = await client.PostAsJsonAsync("/api/events/register", request3);
        Assert.Equal(HttpStatusCode.Conflict, response3.StatusCode);
    }
}
