using Korga.Server.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
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

    public async ValueTask<ChurchToolsResponse<Group[]>> GetGroups(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ChurchToolsResponse<Group[]>>("/api/groups", cancellationToken) 
            ?? throw new InvalidDataException();
    }

    public async ValueTask<ChurchToolsResponse<GroupMember[]>> GetGroupMembers(int groupId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ChurchToolsResponse<GroupMember[]>>($"/api/groups/{groupId}/members", cancellationToken) 
            ?? throw new InvalidDataException();
    }

    public async ValueTask<ChurchToolsResponse<Person>> GetPerson(int personId, CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<ChurchToolsResponse<Person>>($"/api/persons/{personId}", cancellationToken)
            ?? throw new InvalidDataException();
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
