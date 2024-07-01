using ChurchTools;
using ChurchTools.Model;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Controllers;

[Authorize]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly IChurchToolsApi churchTools;

    public ServiceController(IChurchToolsApi churchTools)
    {
        this.churchTools = churchTools;
    }

    [HttpGet("~/api/services")]
    [ProducesResponseType(typeof(ServiceResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServices()
    {
        var serviceGroupsTask = churchTools.GetServiceGroups();
        var servicesTask = churchTools.GetServices();

        List<ServiceGroup> serviceGroups = await serviceGroupsTask;
        List<Service> services = await servicesTask;

        return new JsonResult(services.Select(service => new ServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            ServiceGroupName = serviceGroups.Find(g => g.Id == service.ServiceGroupId)?.Name
        }));
    }

    [HttpGet("~/api/services/{id}/history")]
    [ProducesResponseType(typeof(ServiceHistoryResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceHistory(int id, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        Dictionary<int, ServiceHistoryResponse> people = (await GetServiceMembers(id)).ToDictionary(x => x.PersonId);

        if (people.Count == 0) return new JsonResult(Array.Empty<ServiceHistoryResponse>());

        from ??= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        to ??= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3));

        List<Event> events = await churchTools.GetEvents(from.Value, to.Value);
        foreach (Event @event in events)
        {
            foreach (Event.Service eventService in @event.EventServices)
            {
                if (eventService.ServiceId == id
                    && eventService.PersonId.HasValue
                    && eventService.Agreed
                    && people.TryGetValue(eventService.PersonId.Value, out var person))
                {
                    person.ServiceDates.Add(DateOnly.FromDateTime(@event.StartDate));
                }
            }
        }

        List<ServiceHistoryResponse> peopleList = people.Values.ToList();
        peopleList.Sort((a, b) => a.ServiceDates.LastOrDefault().CompareTo(b.ServiceDates.LastOrDefault()));
        return new JsonResult(peopleList);
    }

    private async ValueTask<IEnumerable<ServiceHistoryResponse>> GetServiceMembers(int serviceId)
    {
        Service service = await churchTools.GetService(serviceId);
        if (service.GroupIds == null) return [];

        var assignableGroups = service.GroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse);

        List<GroupMember>[] groupsMembers = await Task.WhenAll(assignableGroups
            .Select(groupId => churchTools.GetGroupMembers(groupId).AsTask()));

        return groupsMembers.SelectMany(x => x).GroupBy(
            x => x.PersonId,
            x => new
            {
                FirstName = x.Person.DomainAttributes["firstName"].GetValue<string>(),
                LastName = x.Person.DomainAttributes["lastName"].GetValue<string>(),
                GroupName = x.Group.Title,
                x.GroupMemberStatus,
                x.Comment
            },
            (key, x) => new ServiceHistoryResponse
            {
                PersonId = key,
                FirstName = x.First().FirstName,
                LastName = x.First().LastName,
                Groups = x.Select(y => new ServiceHistoryResponse.GroupInfo()
                {
                    GroupName = y.GroupName,
                    GroupMemberStatus = y.GroupMemberStatus,
                    Comment = y.Comment
                }).ToList()
            });
    }
}
