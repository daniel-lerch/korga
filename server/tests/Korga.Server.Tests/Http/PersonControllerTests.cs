﻿using Korga.Server.Models.Json;
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
            var people = await client.GetFromJsonAsync<PersonResponse[]>("/api/people");
            Assert.IsNotNull(people);
            Assert.IsTrue(people!.Length > 0, "No people found. Please make sure to populate the database before testing.");
            Assert.IsTrue(people!.Any(person => person.GivenName == "Karl-Heinz" && person.FamilyName == "Günther" && person.MailAddress == "gunther@example.com"));
        }

        [TestMethod]
        public async Task TestCreatePerson()
        {
            var request = new PersonRequest("Lara", "Croft", mailAddress: null);
            var response = await client.PostAsJsonAsync("/api/person/new", request);
            var person = await response.Content.ReadFromJsonAsync<PersonResponse2>();
            Assert.IsNotNull(person);
            Assert.AreNotEqual(0, person!.Id);
            Assert.AreEqual("Lara", person!.GivenName);
            Assert.AreEqual("Croft", person!.FamilyName);
            Assert.AreNotEqual(default, person!.CreationTime);
        }

        [TestMethod]
        public async Task TestUpdatePerson_NoChange()
        {
            var person = await client.GetFromJsonAsync<PersonResponse2>("/api/person/1");
            Assert.IsNotNull(person);
            var request = new PersonRequest(person!.GivenName, person!.FamilyName, person!.MailAddress);
            var response = await client.PutAsJsonAsync("/api/person/1", request);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
