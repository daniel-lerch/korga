using Korga.ChurchTools.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.ChurchTools;

public class ChurchToolsApi : IChurchToolsApi, IDisposable
{
    public static async ValueTask<ChurchToolsApi> Login(string host, string username, string password)
    {
        HttpClient httpClient = new(new HttpClientHandler { UseCookies = false });

        try
        {
            return await Login(new(), host, username, password);
        }
        catch
        {
            httpClient.Dispose();
            throw;
        }
    }

    public static async ValueTask<ChurchToolsApi> Login(HttpClient httpClient, string host, string username, string password)
    {
        httpClient.BaseAddress = new UriBuilder("https", host).Uri;

        HttpResponseMessage response =
            await httpClient.PostAsJsonAsync("/api/login", new { Username = username, Password = password, RememberMe = false });

        response.EnsureSuccessStatusCode();

        if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? cookieHeaders))
            throw new InvalidOperationException($"Login response from {httpClient.BaseAddress} is missing a Set-Cookie header. Make sure that you set UseCookies to false in your HttpMessageHandler.");

        Response<Login> login = await response.Content.ReadFromJsonAsync<Response<Login>>()
            ?? throw new InvalidDataException();

        CookieContainer cookieContainer = new();

        foreach (string cookie in cookieHeaders)
        {
            cookieContainer.SetCookies(httpClient.BaseAddress, cookie);
        }

        httpClient.DefaultRequestHeaders.Add("Cookie", cookieContainer.GetCookieHeader(httpClient.BaseAddress));

        return new ChurchToolsApi(httpClient, login.Data);
    }

    public static ChurchToolsApi CreateWithToken(string host, string token) => CreateWithToken(new(), host, token);

    public static ChurchToolsApi CreateWithToken(HttpClient httpClient, string host, string loginToken)
    {
        httpClient.BaseAddress = new UriBuilder("https", host).Uri;
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Login", loginToken);

        return new ChurchToolsApi(httpClient, null);
    }

    private readonly HttpClient httpClient;

    private ChurchToolsApi(HttpClient httpClient, Login? user)
    {
        this.httpClient = httpClient;
        User = user;
    }

    public Login? User { get; }

    public ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken = default)
    {
        return InternalGetAllPages<Group>("/api/groups", "&show_overdue_groups=true&show_inactive_groups=true", cancellationToken);
    }

    public ValueTask<List<Group>> GetGroups(IEnumerable<int> groupStatuses, CancellationToken cancellationToken = default)
    {
        StringBuilder query = new("&show_overdue_groups=true&show_inactive_groups=true");
        foreach (int groupStatus in groupStatuses)
        {
            query.Append("&group_status_ids[]=");
            query.Append(groupStatus);
        }
        return InternalGetAllPages<Group>("/api/groups", query.ToString(), cancellationToken);
    }

    public ValueTask<List<GroupMember>> GetGroupMembers(CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<List<GroupMember>>("/api/groups/members", cancellationToken);
    }

    public ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken = default)
    {
        return InternalGetAllPages<Person>("/api/persons", null, cancellationToken);
    }

    public ValueTask<Person> GetPerson(CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<Person>("/api/whoami", cancellationToken);
    }

    public ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<Person>($"/api/persons/{personId}", cancellationToken);
    }

    public ValueTask<string> GetPersonLoginToken(int personId, CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<string>($"/api/persons/{personId}/logintoken", cancellationToken);
    }

    public ValueTask<PersonMasterdata> GetPersonMasterdata(CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<PersonMasterdata>("/api/person/masterdata", cancellationToken);
    }

    public ValueTask<List<Service>> GetServices(CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<List<Service>>("/api/services", cancellationToken);
    }

    public ValueTask<Service> GetService(int serviceId, CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<Service>($"/api/services/{serviceId}", cancellationToken);
    }

    public ValueTask<List<ServiceGroup>> GetServiceGroups(CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<List<ServiceGroup>>("/api/servicegroups", cancellationToken);
    }

    public ValueTask<List<Event>> GetEvents(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        string path = $"/api/events?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&include=eventServices";
        return InternalGetNonPaged<List<Event>>(path, cancellationToken);
    }

    public ValueTask<GlobalPermissions> GetGlobalPermissions(CancellationToken cancellationToken = default)
    {
        return InternalGetNonPaged<GlobalPermissions>("/api/permissions/global", cancellationToken);
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    private async ValueTask<T> InternalGetNonPaged<T>(string path, CancellationToken cancellationToken)
    {
        Response<T> response = await httpClient.GetFromJsonAsync<Response<T>>(path, cancellationToken)
            ?? throw new InvalidDataException($"ChurchTools endpoint {path} returned an invalid response");

        return response.Data;
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
