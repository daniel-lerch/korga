using Korga.Filters;
using Korga.Filters.Entities;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Korga.Controllers;

[Authorize]
[ApiController]
public class PermissionController : ControllerBase
{
    private readonly DatabaseContext database;
    private readonly PersonFilterService filterService;

    public PermissionController(DatabaseContext database, PersonFilterService filterService)
    {
        this.database = database;
        this.filterService = filterService;
    }

    [HttpGet("~/api/permissions")]
    public async Task<IActionResult> Get()
    {
        if (!await filterService.HasPermission(User, "permissions:view"))
            return StatusCode(StatusCodes.Status403Forbidden);

        List<Permission> permissions = await database.Permissions.Include(p => p.PersonFilterList).ToListAsync();

        List<PermissionResponse> response = [];

        foreach (Permission permission in permissions)
        {
            List<PersonFilterResponse> filters = [];

            foreach (PersonFilter personFilter in permission.PersonFilterList?.Filters ?? [])
            {
                filters.Add(await filterService.GetFilterResponse(personFilter));
            }

            response.Add(new()
            {
                Key = permission.Key,
                PersonFilters = filters,
            });
        }

        return new JsonResult(response);
    }
}
