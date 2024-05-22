using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Tests.Http;

public class VueSpaTests
{
    [Theory]
    [InlineData("", "/")]
    [InlineData("", "/events/list")]
    [InlineData("/korga/test/pathbase", "/")]
    [InlineData("/korga/test/pathbase", "/events/list")]
    public async Task TestVueEntrypoint(string pathbase, string url)
    {
        TestServer server = TestHost.CreateTestServer(new Dictionary<string, string?> { { "Hosting:PathBase", pathbase } });
        HttpClient client = server.CreateClient();

        var response = await client.GetAsync(pathbase + url);

        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        string body = await response.Content.ReadAsStringAsync();
        Assert.True(body.StartsWith("<!DOCTYPE html>"), "Body is missing DOCTYPE");
        Assert.True(body.Contains($"window.resourceBasePath = '{pathbase}/'"), "Body is missing pathbase");
    }
}
