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

    [HttpGet("~/api/services/history")]
    [ProducesResponseType(typeof(ServiceHistoryResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceHistory([FromQuery] HashSet<int> serviceId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var groupIds = await GetServiceGroups(serviceId);
        var groupMembers = await GetGroupMembers(groupIds);

        Dictionary<int, ServiceHistoryResponse> people = groupMembers.ToDictionary(x => x.PersonId);

        if (people.Count == 0) return new JsonResult(Array.Empty<ServiceHistoryResponse>());

        from ??= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-12));
        to ??= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3));

        List<Event> events = await churchTools.GetEvents(from.Value, to.Value);
        foreach (Event @event in events)
        {
            foreach (Event.Service eventService in @event.EventServices)
            {
                if (serviceId.Contains(eventService.ServiceId)
                    && eventService.PersonId.HasValue
                    && eventService.Agreed
                    && people.TryGetValue(eventService.PersonId.Value, out var person))
                {
                    person.ServiceDates.Add(new()
                    {
                        ServiceId = eventService.ServiceId,
                        Date = DateOnly.FromDateTime(@event.StartDate)
                    });
                }
            }
        }

        return new JsonResult(people.Values.ToList());
    }

    private async ValueTask<IEnumerable<int>> GetServiceGroups(IEnumerable<int> serviceIds)
    {
        Service[] services = await Task.WhenAll(serviceIds.Select(id => churchTools.GetService(id).AsTask()));

        HashSet<int> groupIds = [];
        foreach (Service service in services)
        {
            if (service.GroupIds == null) continue;
            groupIds.UnionWith(service.GroupIds);
        }
        return groupIds;
    }

    private async ValueTask<IEnumerable<ServiceHistoryResponse>> GetGroupMembers(IEnumerable<int> groupIds)
    {
        List<GroupMember>[] groupsMembers = await Task.WhenAll(groupIds
            .Select(groupId => churchTools.GetGroupMembers(groupId).AsTask()));

        return groupsMembers.SelectMany(x => x).GroupBy(
            x => x.PersonId,
            x => new
            {
                FirstName = x.Person.DomainAttributes["firstName"].GetValue<string>(),
                LastName = x.Person.DomainAttributes["lastName"].GetValue<string>(),
                GroupId = int.Parse(x.Group.DomainIdentifier),
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
                    GroupId = y.GroupId,
                    GroupName = y.GroupName,
                    GroupMemberStatus = y.GroupMemberStatus,
                    Comment = y.Comment ?? string.Empty,
                }).ToList()
            });
    }
}
