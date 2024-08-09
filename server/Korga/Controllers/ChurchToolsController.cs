using Korga.Filters;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Controllers;

[Authorize]
[ApiController]
public class ChurchToolsController : ControllerBase
{
    private readonly DatabaseContext database;
    private readonly PersonFilterService filterService;

    public ChurchToolsController(DatabaseContext database, PersonFilterService filterService)
    {
        this.database = database;
        this.filterService = filterService;
    }

    [HttpGet("~/api/statuses")]
    [ProducesResponseType(typeof(StatusResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStatuses()
    {
        if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin) && !await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var statuses = await database.Status
            .Where(s => s.DeletionTime == default)
            .Select(s => new StatusResponse
            {
                Id = s.Id,
                Name = s.Name,
            })
            .ToListAsync();

        return new JsonResult(statuses);
    }

    [HttpGet("~/api/groups")]
    [ProducesResponseType(typeof(GroupResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetGroups()
    {
        if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin) && !await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var groups = await database.Groups
            .Where(g => g.DeletionTime == default)
            .Select(g => new GroupResponse
            {
                Id = g.Id,
                GroupTypeId = g.GroupTypeId,
                Name = g.Name,
            })
            .ToListAsync();

        return new JsonResult(groups);
    }

    [HttpGet("~/api/group-types")]
    [ProducesResponseType(typeof(GroupTypeResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetGroupTypes()
    {
        if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin) && !await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var groupTypes = await database.GroupTypes
            .Where(t => t.DeletionTime == default)
            .Select(t => new GroupTypeResponse
            {
                Id = t.Id,
                Name = t.Name,
            })
            .ToListAsync();

        return new JsonResult(groupTypes);
    }

    [HttpGet("~/api/group-roles")]
    [ProducesResponseType(typeof(GroupRoleResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetGroupRoles()
    {
        if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin) && !await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var groupRoles = await database.GroupRoles
            .Where(r => r.DeletionTime == default)
            .Select(r => new GroupRoleResponse
            {
                Id = r.Id,
                GroupTypeId = r.GroupTypeId,
                Name = r.Name,
            })
            .ToListAsync();

        return new JsonResult(groupRoles);
    }

    [HttpGet("~/api/people")]
    [ProducesResponseType(typeof(PersonResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPeople()
    {
        if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin) && !await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var people = await database.People
            .Where(p => p.DeletionTime == default)
            .Select(p => new PersonResponse
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
            })
            .ToListAsync();

        return new JsonResult(people);
    }
}
