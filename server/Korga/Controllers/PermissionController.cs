using Korga.Extensions;
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
    [ProducesResponseType(typeof(PermissionResponse[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get()
    {
        if (!await filterService.HasPermission(User, Permissions.Permissions_View) && !await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        List<Permission> permissions =
            await database.Permissions.Include(p => p.PersonFilterList).ThenInclude(l => l!.Filters).ToListAsync();

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

    [HttpPost("~/api/permission/{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddFilter(Permissions key, [FromBody] PersonFilterRequest request)
    {
        if (!await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        Permission? permission = await database.Permissions.Include(p => p.PersonFilterList).FirstOrDefaultAsync(p => p.Key == key);

        if (permission == null)
            return StatusCode(StatusCodes.Status404NotFound);

        try
        {
            PersonFilter filter = request.ToEntity();
            filter.PersonFilterListId = permission.PersonFilterList!.Id;
            database.PersonFilters.Add(filter);
            await database.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyConstraintViolation())
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return StatusCode(StatusCodes.Status409Conflict);
        }

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpDelete("~/api/permission/{key}/{filterId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFilter(Permissions key, int filterId)
    {
        if (!await filterService.HasPermission(User, Permissions.Permissions_Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        Permission? permission = await database.Permissions.FirstOrDefaultAsync(p => p.Key == key);

        if (permission == null)
            return StatusCode(StatusCodes.Status404NotFound);

        PersonFilter? filter = await database.PersonFilters.SingleOrDefaultAsync(f => f.Id == filterId && f.PersonFilterListId == permission.PersonFilterListId);

        if (filter == null)
            return StatusCode(StatusCodes.Status404NotFound);

        database.PersonFilters.Remove(filter);
        await database.SaveChangesAsync();

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
