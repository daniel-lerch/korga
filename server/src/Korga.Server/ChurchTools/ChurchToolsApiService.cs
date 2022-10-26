using Korga.Server.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Korga.Server.ChurchTools;

public class ChurchToolsApiService : IDisposable
{
    private readonly HttpClient httpClient;

    public ChurchToolsApiService(IOptions<EmailRelayOptions> options)
    {
        httpClient = new();
        httpClient.BaseAddress = new UriBuilder("https", options.Value.ChurchToolsHost).Uri;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Login", options.Value.ChurchToolsLoginToken);
    }

    public Task<ChurchToolsResponse<Group[]>?> GetGroups()
    {
        return httpClient.GetFromJsonAsync<ChurchToolsResponse<Group[]>>("/api/groups");
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
