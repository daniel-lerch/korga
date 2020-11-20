using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Korga.Server.Tests.Http
{
    [TestClass]
    public class PersonControllerTests
    {
        // These variables are set by the test host
        private IServiceProvider serviceProvider = null!;
        private IServiceScope scope = null!;
        private DatabaseContext database = null!;
        private TestServer server = null!;
        private HttpClient client = null!;

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void Initialize()
        {
            serviceProvider = TestHost.CreateServiceCollection().BuildServiceProvider();
            scope = serviceProvider.CreateScope();
            database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            server = TestHost.CreateTestServer();
            client = server.CreateClient();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            this.scope.Dispose();
            server.Dispose();
            client.Dispose();

            var scope = serviceProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            database.RemoveRange(await database.People.AsTracking()
                .Where(p => p.MailAddress == $"{TestContext.TestName.ToLowerInvariant()}@unittest.example.com")
                .ToArrayAsync());
            await database.SaveChangesAsync();

            scope.Dispose();
        }

        [TestMethod]
        public async Task TestGetPeople()
        {
            string address = $"{nameof(TestGetPeople).ToLowerInvariant()}@unittest.example.com";

            database.People.Add(new Person("Max", "Mustermann") { MailAddress = address });
            await database.SaveChangesAsync();

            var people = await client.GetFromJsonAsync<PersonResponse[]>("/api/people") ?? throw new AssertFailedException();
            Assert.IsTrue(people.Length > 0, "No people found. Please make sure to populate the database before testing.");
            Assert.IsTrue(people.Any(person => person.GivenName == "Max" && person.FamilyName == "Mustermann" && person.MailAddress == address));
        }

        [TestMethod]
        public async Task TestGetPerson()
        {
            var person = await client.GetFromJsonAsync<PersonResponse2>("/api/person/1") ?? throw new AssertFailedException();
            Assert.AreNotEqual(default, person.CreationTime);
            Assert.IsNull(person.CreatedBy);
            Assert.AreEqual(1, person.Memberships.Count);
            Assert.IsNotNull(person.Memberships[0].CreatedBy);
            Assert.AreEqual(person.GivenName, person.Memberships[0].CreatedBy!.GivenName);
            Assert.AreEqual(person.FamilyName, person.Memberships[0].CreatedBy!.FamilyName);
            Assert.AreEqual(0, person.History.Count);
        }

        [TestMethod]
        public async Task TestCreatePerson()
        {
            string address = $"{nameof(TestCreatePerson).ToLowerInvariant()}@unittest.example.com";

            var request = new PersonRequest("Max", "Mustermann", mailAddress: address);
            var response = await client.PostAsJsonAsync("/api/person/new", request);
            var person = await response.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreNotEqual(0, person.Id);
            Assert.AreEqual("Max", person.GivenName);
            Assert.AreEqual("Mustermann", person.FamilyName);
            Assert.AreEqual(address, person.MailAddress);
            Assert.AreNotEqual(default, person.CreationTime);
        }

        [TestMethod]
        public async Task TestUpdatePerson_NoChange()
        {
            var person = await client.GetFromJsonAsync<PersonResponse2>("/api/person/1") ?? throw new AssertFailedException();

            var request = new PersonRequest(person.GivenName, person.FamilyName, person.MailAddress);
            var response = await client.PutAsJsonAsync("/api/person/1", request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var person2 = await response.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreEqual(person.History.Count, person2.History.Count);
        }

        [TestMethod]
        public async Task TestUpdatePerson_NonConcurrent()
        {
            string address = $"{nameof(TestUpdatePerson_NonConcurrent).ToLowerInvariant()}@unittest.example.com";

            var person = new Person("Max", "Mustermann") { MailAddress = address };
            database.People.Add(person);
            await database.SaveChangesAsync();

            var request = new PersonRequest("Maximilian", "Mustermann", address);
            var response = await client.PutAsJsonAsync($"/api/person/{person.Id}", request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var person2 = await response.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreEqual(1, person2.History.Count);
        }

        [TestMethod]
        public async Task TestUpdatePerson_Deleted()
        {
            string address = $"{nameof(TestUpdatePerson_Deleted).ToLowerInvariant()}@unittest.example.com";

            var person = new Person("Max", "Mustermann")
            {
                MailAddress = address,
                CreationTime = DateTime.UtcNow.AddSeconds(-1),
                DeletionTime = DateTime.UtcNow
            };
            database.People.Add(person);
            await database.SaveChangesAsync();

            var request = new PersonRequest("Maximilian", "Mustermann", address);
            var response = await client.PutAsJsonAsync($"/api/person/{person.Id}", request);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

            var person2 = await response.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreEqual(0, person2.History.Count);
        }
    }
}
