using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.TestHost;
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
    public class PersonControllerTests : DatabaseTest
    {
        // These variables are set by the test host
        private TestServer server = null!;
        private HttpClient client = null!;

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();

            server = TestHost.CreateTestServer();
            client = server.CreateClient();
        }

        [TestCleanup]
        public override async Task Cleanup()
        {
            server.Dispose();
            client.Dispose();

            await base.Cleanup();
        }

        [TestMethod]
        public async Task TestGetPeople()
        {
            string address = GenerateMailAddress();

            database.People.Add(new Person("Max", "Mustermann") { MailAddress = address });
            database.People.Add(new Person("Paul", "Ehrlich")
            {
                MailAddress = address,
                CreationTime = DateTime.UtcNow.AddSeconds(-10),
                DeletionTime = DateTime.UtcNow.AddSeconds(-5)
            });
            await database.SaveChangesAsync();

            var people = await client.GetFromJsonAsync<PersonResponse[]>("/api/people") ?? throw new AssertFailedException();
            Assert.IsTrue(people.Length >= 2, "No people found. Please make sure to populate the database before testing.");
            Assert.IsTrue(people.Any(p => p.GivenName == "Max" && p.FamilyName == "Mustermann" && p.MailAddress == address && !p.Deleted));
            Assert.IsTrue(people.Any(p => p.GivenName == "Paul" && p.FamilyName == "Ehrlich" && p.MailAddress == address && p.Deleted));
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
            string address = GenerateMailAddress();

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
            string address = GenerateMailAddress();

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
            string address = GenerateMailAddress();

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
