using Korga.Server.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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

    public async ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken)
    {
        List<Group> groups = new();
        ChurchToolsResponse<Group[]>? response;
        int page = 0;

        do
        {
            response = await httpClient.GetFromJsonAsync<ChurchToolsResponse<Group[]>>($"/api/groups?page={++page}&limit=100", cancellationToken);
            
            if (response == null || response.Meta == null)
                throw new InvalidDataException();

            groups.AddRange(response.Data);

        } while (response.Meta.Pagination.Current < response.Meta.Pagination.LastPage);

        return groups;
    }

    public async ValueTask<List<GroupMember>> GetGroupMembers(int groupId, CancellationToken cancellationToken)
    {
        List<GroupMember> members = new();
        ChurchToolsResponse<GroupMember[]>? response;
        int page = 0;

        do
        {
            response = await httpClient.GetFromJsonAsync<ChurchToolsResponse<GroupMember[]>>($"/api/groups/{groupId}/members?page={++page}&limit=100", cancellationToken);
            
            if (response == null || response.Meta == null)
                throw new InvalidDataException();

            members.AddRange(response.Data);

        } while (response.Meta.Pagination.Current < response.Meta.Pagination.LastPage);

        return members;
    }

    public async ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken)
    {
        ChurchToolsResponse<Person> person = await httpClient.GetFromJsonAsync<ChurchToolsResponse<Person>>($"/api/persons/{personId}", cancellationToken)
            ?? throw new InvalidDataException();

        return person.Data;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
