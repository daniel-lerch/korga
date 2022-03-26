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
    private readonly long eventId;
    private readonly long programId;

    public EventControllerTests()
    {
        server = TestHost.CreateTestServer();
        client = server.CreateClient();
        serviceScope = server.Services.CreateScope();
        database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var @event = new Event(nameof(EventControllerTests));
        database.Events.Add(@event);
        database.SaveChanges();
        eventId = @event.Id;

        var program = new EventProgram(nameof(EventControllerTests)) { EventId = eventId, Limit = 2 };
        database.EventPrograms.Add(program);
        database.SaveChanges();
        programId = program.Id;
    }

    public void Dispose()
    {
        Event? @event = database.Events.SingleOrDefault(e => e.Id == eventId);
        if (@event is not null)
        {
            database.Events.Remove(@event);
            database.SaveChanges();
        }
        serviceScope.Dispose();
    }

    [Fact]
    public async Task TestRegister()
    {
        var request = new EventRegistrationRequest("Max", "Mustermann") { ProgramId = programId };
        var response = await client.PostAsJsonAsync("/api/events/register", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task TestRegister_TooMany()
    {
        EventRegistrationRequest request1 = new("Johannes", "Schäfer") { ProgramId = programId };
        var response1 = await client.PostAsJsonAsync("/api/events/register", request1);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

        EventRegistrationRequest request2 = new("Katharina", "Schäfer") { ProgramId = programId };
        var response2 = await client.PostAsJsonAsync("/api/events/register", request2);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);

        EventRegistrationRequest request3 = new("Anna", "Schäfer") { ProgramId = programId };
        var response3 = await client.PostAsJsonAsync("/api/events/register", request3);
        Assert.Equal(HttpStatusCode.Conflict, response3.StatusCode);
    }
}
