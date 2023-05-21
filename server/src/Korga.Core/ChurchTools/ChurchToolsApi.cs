using Korga.ChurchTools.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.ChurchTools;

public class ChurchToolsApi : IChurchToolsApi, IDisposable
{
    public static ValueTask<ChurchToolsApi> Login(string host, string username, string password) => Login(new(), host, username, password);

    public static async ValueTask<ChurchToolsApi> Login(HttpClient httpClient, string host, string username, string password)
    {
        httpClient.BaseAddress = new UriBuilder("https", host).Uri;

        HttpResponseMessage response =
            await httpClient.PostAsJsonAsync("/api/login", new { Username = username, Password = password, RememberMe = false });

        response.EnsureSuccessStatusCode();

        if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookieHeaders))
            throw new InvalidOperationException($"Login response from {httpClient.BaseAddress} is missing a Set-Cookie header. Make sure that you set UseCookies to false in your HttpMessageHandler.");

        CookieContainer cookieContainer = new();

        foreach (string cookie in cookieHeaders)
        {
            cookieContainer.SetCookies(httpClient.BaseAddress, cookie);
        }

        httpClient.DefaultRequestHeaders.Add("Cookie", cookieContainer.GetCookieHeader(httpClient.BaseAddress));

        return new ChurchToolsApi(httpClient);
    }

    public static ChurchToolsApi CreateWithToken(string host, string token) => CreateWithToken(new(), host, token);

    public static ChurchToolsApi CreateWithToken(HttpClient httpClient, string host, string loginToken)
    {
        httpClient.BaseAddress = new UriBuilder("https", host).Uri;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Login", loginToken);

        return new ChurchToolsApi(httpClient);
    }

    private readonly HttpClient httpClient;

    private ChurchToolsApi(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken)
    {
        return InternalGetAllPages<Group>("/api/groups", "&show_overdue_groups=true&show_inactive_groups=true", cancellationToken);
    }

    public async ValueTask<List<GroupMember>> GetGroupMembers(CancellationToken cancellationToken)
    {
        Response<List<GroupMember>> groupMembers = await httpClient.GetFromJsonAsync<Response<List<GroupMember>>>("/api/groups/members", cancellationToken)
            ?? throw new InvalidDataException();

        return groupMembers.Data;
    }

    public ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken)
    {
        return InternalGetAllPages<Person>("/api/persons", null, cancellationToken);
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

    private async ValueTask<List<T>> InternalGetAllPages<T>(string path, string? query, CancellationToken cancellationToken)
    {
        List<T> items = new();
        PaginatedResponse<T[]>? response;
        int page = 0;

        do
        {
            response = await httpClient.GetFromJsonAsync<PaginatedResponse<T[]>>($"{path}?page={++page}&limit=100{query}", cancellationToken);

            if (response == null || response.Meta == null)
                throw new InvalidDataException();

            items.AddRange(response.Data);

        } while (response.Meta.Pagination.Current < response.Meta.Pagination.LastPage);

        return items;
    }
}
