using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests.Http
{
    public class VueSpaTests
    {
        [Fact]
        public async Task TestDefault()
        {
            TestServer server = TestHost.CreateTestServer(new Dictionary<string, string?> { { "Hosting:PathBase", null } });
            HttpClient client = server.CreateClient();

            var response = await client.GetAsync("/");
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
            string body = await response.Content.ReadAsStringAsync();
            Assert.True(body.StartsWith("<!DOCTYPE html>"), "Body is missing DOCTYPE");
            Assert.True(body.Contains("window.resourceBasePath = '/'"), "Body is missing pathbase");
        }

        [Fact]
        public async Task TestPathbase()
        {
            const string pathbase = "/korga/test/pathbase";

            TestServer server = TestHost.CreateTestServer(new Dictionary<string, string?> { { "Hosting:PathBase", pathbase } });
            HttpClient client = server.CreateClient();

            var response = await client.GetAsync(pathbase);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
            string body = await response.Content.ReadAsStringAsync();
            Assert.True(body.StartsWith("<!DOCTYPE html>"), "Body is missing DOCTYPE");
            Assert.True(body.Contains($"window.resourceBasePath = '{pathbase}/'"), "Body is missing pathbase");
        }
    }
}
