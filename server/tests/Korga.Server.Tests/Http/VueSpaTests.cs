using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Korga.Server.Tests.Http
{
    [TestClass]
    public class VueSpaTests
    {
        [TestMethod]
        public async Task TestDefault()
        {
            TestServer server = TestHost.CreateTestServer(new Dictionary<string, string?> { { "Hosting:PathBase", null } });
            HttpClient client = server.CreateClient();

            var response = await client.GetAsync("/");
            Assert.AreEqual("text/html", response.Content.Headers.ContentType.MediaType);
            string body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.StartsWith("<!DOCTYPE html>"), "Body is missing DOCTYPE");
            Assert.IsTrue(body.Contains("window.resourceBasePath = '/'"), "Body is missing pathbase");
        }

        [TestMethod]
        public async Task TestPathbase()
        {
            const string pathbase = "/korga/test/pathbase";

            TestServer server = TestHost.CreateTestServer(new Dictionary<string, string?> { { "Hosting:PathBase", pathbase } });
            HttpClient client = server.CreateClient();

            var response = await client.GetAsync(pathbase);
            Assert.AreEqual("text/html", response.Content.Headers.ContentType.MediaType);
            string body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.StartsWith("<!DOCTYPE html>"), "Body is missing DOCTYPE");
            Assert.IsTrue(body.Contains($"window.resourceBasePath = '{pathbase}/'"), "Body is missing pathbase");
        }
    }
}
