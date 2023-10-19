using Korga.ChurchTools;
using Korga.ChurchTools.Api;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Controllers;

[Authorize]
[ApiController]
public class ServiceController : ControllerBase
{
    private readonly DatabaseContext database;
    private readonly IChurchToolsApi churchTools;

    public ServiceController(DatabaseContext database, IChurchToolsApi churchTools)
    {
        this.database = database;
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
        Service service = await churchTools.GetService(id);
        List<int> groupIds = service.GroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var people = await
            (from member in database.GroupMembers.Where(member => groupIds.Contains(member.GroupId))
             join person in database.People on member.PersonId equals person.Id
             select new ServiceHistoryResponse
             {
                 PersonId = person.Id,
                 FirstName = person.FirstName,
                 LastName = person.LastName
             })
            .Distinct()
            .ToDictionaryAsync(x => x.PersonId);

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
}
