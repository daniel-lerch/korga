using Korga.Server.Models.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private TestServer server = null!;
        private HttpClient client = null!;

        [TestInitialize]
        public void Initialize()
        {
            server = TestHost.CreateTestServer();
            client = server.CreateClient();
        }

        [TestMethod]
        public async Task TestGetPeople()
        {
            var people = await client.GetFromJsonAsync<PersonResponse[]>("/api/people") ?? throw new AssertFailedException();
            Assert.IsTrue(people.Length > 0, "No people found. Please make sure to populate the database before testing.");
            Assert.IsTrue(people.Any(person => person.GivenName == "Karl-Heinz" && person.FamilyName == "Günther" && person.MailAddress == "gunther@example.com"));
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
            var request = new PersonRequest("Lara", "Croft", mailAddress: null);
            var response = await client.PostAsJsonAsync("/api/person/new", request);
            var person = await response.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreNotEqual(0, person.Id);
            Assert.AreEqual("Lara", person.GivenName);
            Assert.AreEqual("Croft", person.FamilyName);
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
            var person = await client.GetFromJsonAsync<PersonResponse2>("/api/person/5") ?? throw new AssertFailedException();

            string? oldMailAddress = person.MailAddress;
            var request1 = new PersonRequest(person.GivenName, person.FamilyName, "unit.test@example.com");
            var response1 = await client.PutAsJsonAsync("/api/person/5", request1);
            Assert.AreEqual(HttpStatusCode.OK, response1.StatusCode);

            var person2 = await response1.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreEqual(person.History.Count + 1, person2.History.Count);

            var request2 = new PersonRequest(person.GivenName, person.FamilyName, oldMailAddress);
            var response2 = await client.PutAsJsonAsync("/api/person/5", request2);
            Assert.AreEqual(HttpStatusCode.OK, response2.StatusCode);

            var person3 = await response2.Content.ReadFromJsonAsync<PersonResponse2>() ?? throw new AssertFailedException();
            Assert.AreEqual(person2.History.Count + 1, person3.History.Count);
        }
    }
}
