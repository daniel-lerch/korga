using Korga.Server.Database;
using Korga.Server.Database.Entities;
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

        testEvent = new Event(Guid.NewGuid().ToString()) { Start = DateTime.Parse("2022-03-26T16:02:31"), End = DateTime.Parse("2022-03-26T16:08:52") };
        database.Events.Add(testEvent);
        database.SaveChanges();

        testProgram = new EventProgram(Guid.NewGuid().ToString())
        {
            EventId = testEvent.Id,
            RegistrationStart = DateTime.Parse("2022-03-25T07:55:00"),
            RegistrationDeadline = DateTime.Parse("2022-03-26T15:10:26"),
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
        Assert.Equal(testEvent.Start, response.Start);
        Assert.Equal(testEvent.End, response.End);
        var program = response.Programs.Single();
        Assert.Equal(testProgram.Id, program.Id);
        Assert.Equal(testProgram.Name, program.Name);
        Assert.Equal(testProgram.RegistrationStart, program.RegistrationStart);
        Assert.Equal(testProgram.RegistrationDeadline, program.RegistrationDeadline);
        Assert.Equal(testProgram.Limit, program.Limit);
        Assert.Equal(0, program.Count);
    }

    [Fact]
    public async Task TestGetEvent_Full()
    {
        var registration = new EventRegistration
        {
            EventId = testEvent.Id,
            Token = Guid.NewGuid(),
            Participants = new EventParticipant[]
            {
                new("Katharina", "Schäfer") { ProgramId = testProgram.Id },
                new("Anna", "Schäfer") { ProgramId = testProgram.Id }
            }
        };
        database.EventRegistrations.Add(registration);
        await database.SaveChangesAsync();
        //database.SaveChanges();

        var response = await client.GetFromJsonAsync<EventResponse2>($"/api/event/{testEvent.Id}");
        Assert.NotNull(response);
        Assert.Equal(testEvent.Name, response.Name);
        Assert.Equal(testEvent.Start, response.Start);
        Assert.Equal(testEvent.End, response.End);
        var program = response.Programs.Single();
        Assert.Equal(testProgram.Id, program.Id);
        Assert.Equal(testProgram.Name, program.Name);
        Assert.Equal(testProgram.RegistrationStart, program.RegistrationStart);
        Assert.Equal(testProgram.RegistrationDeadline, program.RegistrationDeadline);
        Assert.Equal(testProgram.Limit, program.Limit);
        Assert.Equal(2, program.Participants.Count);
        Assert.NotEqual(0, program.Participants.Single(p => p.GivenName == "Katharina" && p.FamilyName == "Schäfer").Id);
        Assert.NotEqual(0, program.Participants.Single(p => p.GivenName == "Anna" && p.FamilyName == "Schäfer").Id);
    }

    [Fact]
    public async Task TestRegister_Single()
    {
        var request = new EventRegistrationRequest[] { new("Max", "Mustermann") { ProgramId = testProgram.Id } };
        var response = await client.PostAsJsonAsync($"/api/event/{testEvent.Id}/register", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var registration = await response.Content.ReadFromJsonAsync<EventRegistrationResponse>();
        Assert.NotNull(registration);
        Assert.NotEqual(Guid.Empty, registration.Token);
    }

    [Fact]
    public async Task TestRegister_TooMany()
    {
        var request = new EventRegistrationRequest[]
        {
            new("Johannes", "Schäfer") { ProgramId = testProgram.Id },
            new("Katharina", "Schäfer") { ProgramId = testProgram.Id },
            new("Anna", "Schäfer") { ProgramId = testProgram.Id },
        };
        var response = await client.PostAsJsonAsync($"/api/event/{testEvent.Id}/register", request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
