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

    [HttpGet("~/api/groups")]
    public async Task<IActionResult> GetGroups()
    {
        if (!await filterService.HasPermission(User, "distribution-lists:modify") && !await filterService.HasPermission(User, "permissions:modify"))
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
    public async Task<IActionResult> GetGroupTypes()
    {
        if (!await filterService.HasPermission(User, "distribution-lists:modify") && !await filterService.HasPermission(User, "permissions:modify"))
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
    public async Task<IActionResult> GetGroupRoles()
    {
        if (!await filterService.HasPermission(User, "distribution-lists:modify") && !await filterService.HasPermission(User, "permissions:modify"))
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
    public async Task<IActionResult> GetPeople()
    {
        if (!await filterService.HasPermission(User, "distribution-lists:modify") && !await filterService.HasPermission(User, "permissions:modify"))
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
