using Korga.Server.ChurchTools.Api;
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

public class ChurchToolsApiService : IChurchToolsApiService
{
    private readonly HttpClient httpClient;

    public ChurchToolsApiService(IOptions<ChurchToolsOptions> options)
    {
        httpClient = new();
        httpClient.BaseAddress = new UriBuilder("https", options.Value.Host).Uri;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Login", options.Value.LoginToken);
    }

    public ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken)
    {
        return InternalGetAllPages<Group>("/api/groups", cancellationToken);
    }

    public ValueTask<List<GroupMember>> GetGroupMembers(int groupId, CancellationToken cancellationToken)
    {
        return InternalGetAllPages<GroupMember>($"/api/groups/{groupId}/members", cancellationToken);
    }

    public ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken)
    {
        return InternalGetAllPages<Person>("/api/persons", cancellationToken);
    }

    public async ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken)
    {
        Response<Person> person = await httpClient.GetFromJsonAsync<Response<Person>>($"/api/persons/{personId}", cancellationToken)
            ?? throw new InvalidDataException();

        return person.Data;
    }

    public async ValueTask<PersonMasterdata> GetPersonMasterdata(CancellationToken cancellationToken)
    {
        Response<PersonMasterdata> personMasterdata = await httpClient.GetFromJsonAsync<Response<PersonMasterdata>>("/api/person/masterdata", cancellationToken)
            ?? throw new InvalidDataException();

        return personMasterdata.Data;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    private async ValueTask<List<T>> InternalGetAllPages<T>(string url, CancellationToken cancellationToken)
    {
		List<T> items = new();
		PaginatedResponse<T[]>? response;
		int page = 0;

		do
		{
			response = await httpClient.GetFromJsonAsync<PaginatedResponse<T[]>>($"{url}?page={++page}&limit=100", cancellationToken);

			if (response == null || response.Meta == null)
				throw new InvalidDataException();

			items.AddRange(response.Data);

		} while (response.Meta.Pagination.Current < response.Meta.Pagination.LastPage);

		return items;
	}
}
