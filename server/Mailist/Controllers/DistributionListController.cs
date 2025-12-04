using ChurchTools;
using ChurchTools.Model;
using Mailist.EmailRelay;
using Mailist.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mailist.Controllers;

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

        //await UpdateCachedRecipientCount(lists);

        List<DistributionList> response = lists
            .Select(dl => new DistributionList
            {
                Id = dl.Id,
                Alias = dl.Alias,
                Newsletter = dl.Flags.HasFlag(DistributionListFlags.Newsletter),
                RecipientsQuery = JsonElement.Parse(dl.RecipientsQuery),
                RecipientCount = dl.RecipientCount,
                RecipientCountTime = dl.RecipientCountTime,
            })
            .ToList();

        return response;
    }

    [Authorize]
    [HttpGet("~/api/distribution-lists/{id:long}")]
    [ProducesResponseType(typeof(DistributionList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDistributionList([FromRoute] long id)
    {
        var distributionList = await database.DistributionLists.FindAsync(id);
        if (distributionList == null)
            return NotFound();

        // Refresh cached recipient count if stale
        await UpdateCachedRecipientCount([distributionList]);

        var response = new DistributionList
        {
            Id = distributionList.Id,
            Alias = distributionList.Alias,
            Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = JsonElement.Parse(distributionList.RecipientsQuery),
            RecipientCount = distributionList.RecipientCount,
            RecipientCountTime = distributionList.RecipientCountTime,
        };

        return Ok(response);
    }

    private async ValueTask UpdateCachedRecipientCount(List<EmailRelay.Entities.DistributionList> lists)
    {
        // Update recipient counts for lists with a query older than one hour
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        var tasks = lists
            .Where(dl => dl.RecipientsQuery != "null" && dl.RecipientCountTime < oneHourAgo)
            .Select(async dl =>
            {
                ChurchQueryRequest<IdNameEmail> query = new(JsonElement.Parse(dl.RecipientsQuery));
                var recipients = await churchTools.ChurchQuery(query);
                dl.RecipientCount = recipients.Count;
                dl.RecipientCountTime = DateTime.UtcNow;
            });

        await Task.WhenAll(tasks);

        await database.SaveChangesAsync();
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

        if (request.RecipientsQuery.ValueKind != JsonValueKind.Null)
        {
            ChurchQueryRequest<IdNameEmail> query = new(request.RecipientsQuery);
            var recipients = await churchTools.ChurchQuery(query);
            recipientCount = recipients.Count;
            recipientCountTime = DateTime.UtcNow;
        }

        var distributionList = new EmailRelay.Entities.DistributionList(request.Alias, request.RecipientsQuery.GetRawText())
        {
            Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None,
            RecipientCount = recipientCount,
            RecipientCountTime = recipientCountTime,
        };

        database.DistributionLists.Add(distributionList);
        await database.SaveChangesAsync();

        var response = new DistributionList
        {
            Id = distributionList.Id,
            Alias = distributionList.Alias,
            Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = JsonElement.Parse(distributionList.RecipientsQuery),
            RecipientCount = recipientCount,
            RecipientCountTime = recipientCountTime,
        };

        return Created($"/api/distribution-lists/{distributionList.Id}", response);
    }

    [Authorize(Roles = "admin")]
    [HttpPut("~/api/distribution-lists/{id:long}")]
    [ProducesResponseType(typeof(DistributionList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModifyDistributionList([FromRoute] long id, [FromBody] CreateDistributionList request)
    {
        if (string.IsNullOrWhiteSpace(request.Alias))
            return BadRequest("Alias must not be empty");

        var distributionList = await database.DistributionLists.FindAsync(id);
        if (distributionList == null)
            return NotFound();

        distributionList.Alias = request.Alias;
        distributionList.Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None;
        distributionList.RecipientsQuery = request.RecipientsQuery.GetRawText();

        if (request.RecipientsQuery.ValueKind == JsonValueKind.Null)
        {
            distributionList.RecipientCount = 0;
            distributionList.RecipientCountTime = default;
        }
        else
        {
            ChurchQueryRequest<IdNameEmail> query = new(request.RecipientsQuery);
            var recipients = await churchTools.ChurchQuery(query);
            distributionList.RecipientCount = recipients.Count;
            distributionList.RecipientCountTime = DateTime.UtcNow;
        }

        await database.SaveChangesAsync();

        var response = new DistributionList
        {
            Id = distributionList.Id,
            Alias = distributionList.Alias,
            Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
            RecipientsQuery = JsonElement.Parse(distributionList.RecipientsQuery),
            RecipientCount = distributionList.RecipientCount,
            RecipientCountTime = distributionList.RecipientCountTime,
        };

        return Ok(response);
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
