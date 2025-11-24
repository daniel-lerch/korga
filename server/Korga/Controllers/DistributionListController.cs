using Korga.EmailRelay;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Korga.Controllers;

[ApiController]
public class DistributionListController : ControllerBase
{
    private readonly DatabaseContext database;

    public DistributionListController(DatabaseContext database)
    {
        this.database = database;
    }

    [Authorize]
    [HttpGet("~/api/distribution-lists")]
    [ProducesResponseType(typeof(DistributionList[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DistributionList>>> GetDistributionLists()
    {
        var lists = await database.DistributionLists
            .AsNoTracking()
            .OrderBy(dl => dl.Alias)
            .ToListAsync();

        List<DistributionList> response = lists
            .Select(dl => new DistributionList(
                dl.Id,
                dl.Alias,
                dl.Flags.HasFlag(DistributionListFlags.Newsletter),
                dl.RecipientsQuery == null ? null : JsonElement.Parse(dl.RecipientsQuery)))
            .ToList();

        return response;
    }

    [Authorize(Roles = "admin")]
    [HttpPost("~/api/distribution-lists")]
    [ProducesResponseType(typeof(DistributionList), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDistributionList([FromBody] CreateDistributionList request)
    {
        if (string.IsNullOrWhiteSpace(request.Alias))
            return BadRequest("Alias must not be empty");

        var distributionList = new EmailRelay.Entities.DistributionList(request.Alias)
        {
            RecipientsQuery = request.RecipientsQuery.ToString(),
            Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None
        };

        database.DistributionLists.Add(distributionList);
        await database.SaveChangesAsync();

        var response = new DistributionList(
            distributionList.Id,
            distributionList.Alias,
            distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            distributionList.RecipientsQuery == null ? null : JsonElement.Parse(distributionList.RecipientsQuery));

        return Created($"/api/distribution-lists/{distributionList.Id}", response);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("~/api/distribution-lists/{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDistributionList([FromRoute] long id)
    {
        var distributionList = await database.DistributionLists.FindAsync(id);

        if (distributionList == null)
            return NotFound();

        database.DistributionLists.Remove(distributionList);
        await database.SaveChangesAsync();

        return NoContent();
    }
}
