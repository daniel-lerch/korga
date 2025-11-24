using ChurchTools;
using ChurchTools.Model;
using Korga.EmailRelay;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Korga.Controllers;

[ApiController]
public class DistributionListController : ControllerBase
{
    private readonly DatabaseContext database;
    private readonly IChurchToolsApi churchTools;

    public DistributionListController(DatabaseContext database, IChurchToolsApi churchTools)
    {
        this.database = database;
        this.churchTools = churchTools;
    }

    [Authorize]
    [HttpGet("~/api/distribution-lists")]
    [ProducesResponseType(typeof(DistributionList[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DistributionList>>> GetDistributionLists()
    {
        var lists = await database.DistributionLists
            .OrderBy(dl => dl.Alias)
            .ToListAsync();

        // Update recipient counts for lists with a query older than one hour
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        await Task.WhenAll(lists.Where(dl => dl.RecipientsQuery != null && dl.RecipientCountTime < oneHourAgo).Select(async dl =>
        {
            ChurchQueryRequest<IdNameEmail> query = new(JsonElement.Parse(dl.RecipientsQuery!));
            var recipients = await churchTools.ChurchQuery(query);
            dl.RecipientCount = recipients.Count;
            dl.RecipientCountTime = DateTime.UtcNow;
        }));
        await database.SaveChangesAsync();

        List<DistributionList> response = lists
            .Select(dl => new DistributionList
            {
                Id = dl.Id,
                Alias = dl.Alias,
                Newsletter = dl.Flags.HasFlag(DistributionListFlags.Newsletter),
                RecipientsQuery = dl.RecipientsQuery == null ? null : JsonElement.Parse(dl.RecipientsQuery),
                RecipientCount = dl.RecipientCount,
                RecipientCountTime = dl.RecipientCountTime,
            })
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

        int recipientCount = 0;
        DateTime recipientCountTime = default;

        if (request.RecipientsQuery.HasValue)
        {
            ChurchQueryRequest<IdNameEmail> query = new(request.RecipientsQuery.Value);
            var recipients = await churchTools.ChurchQuery(query);
            recipientCount = recipients.Count;
            recipientCountTime = DateTime.UtcNow;
        }

        var distributionList = new EmailRelay.Entities.DistributionList(request.Alias)
        {
            Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None,
            RecipientsQuery = request.RecipientsQuery.ToString(),
            RecipientCount = recipientCount,
            RecipientCountTime = recipientCountTime,
        };

        database.DistributionLists.Add(distributionList);
        await database.SaveChangesAsync();

        var response = new DistributionList {
            Id = distributionList.Id,
            Alias = distributionList.Alias,
            Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = distributionList.RecipientsQuery == null ? null : JsonElement.Parse(distributionList.RecipientsQuery),
            RecipientCount = recipientCount,
            RecipientCountTime = recipientCountTime,
        };

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
