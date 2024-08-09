using Korga.EmailRelay;
using Korga.EmailRelay.Entities;
using Korga.Filters;
using Korga.Filters.Entities;
using Korga.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Controllers
{
    [Authorize]
    [ApiController]
    public class DistributionListController : ControllerBase
    {
        private readonly DatabaseContext database;
        private readonly PersonFilterService filterService;

        public DistributionListController(DatabaseContext database, PersonFilterService filterService)
        {
            this.database = database;
            this.filterService = filterService;
        }

        [HttpGet("~/api/distribution-lists")]
        [ProducesResponseType(typeof(DistributionListResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDistributionLists()
        {
            if (!await filterService.HasPermission(User, Permissions.DistributionLists_View) && !await filterService.HasPermission(User, Permissions.DistributionLists_Admin))
                return StatusCode(StatusCodes.Status403Forbidden);

            List<DistributionList> distributionLists = await database.DistributionLists
                .Include(dl => dl.PermittedRecipients)
                .ThenInclude(fl => fl!.Filters)
                .Include(dl => dl.PermittedSenders)
                .ThenInclude(fl => fl!.Filters)
                .OrderBy(dl => dl.Alias)
                .ToListAsync();

            List<DistributionListResponse> response = [];

            foreach (DistributionList distributionList in distributionLists)
            {
                List<PersonFilterResponse> recipients = [];
                List<PersonFilterResponse> senders = [];

                foreach (PersonFilter personFilter in distributionList.PermittedRecipients?.Filters ?? [])
                {
                    PersonFilterResponse filter = await filterService.GetFilterResponse(personFilter);

                    recipients.Add(filter);
                }

                foreach (PersonFilter personFilter in distributionList.PermittedSenders?.Filters ?? [])
                {
                    PersonFilterResponse filter = await filterService.GetFilterResponse(personFilter);

                    senders.Add(filter);
                }

                response.Add(new()
                {
                    Id = distributionList.Id,
                    Alias = distributionList.Alias,
                    Newsletter = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter),
                    PermittedRecipients = recipients,
                    PermittedSenders = senders,
                });
            }

            return new JsonResult(response);
        }

        [HttpPost("~/api/distribution-lists")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateDistributionList([FromBody] DistributionListRequest request)
        {
            if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin))
                return StatusCode(StatusCodes.Status403Forbidden);

            DistributionList distributionList = new(request.Alias)
            {
                Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None,
            };

            database.DistributionLists.Add(distributionList);
            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPut("~/api/distribution-list/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDistributionList(long id, [FromBody] DistributionListRequest request)
        {
            if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin))
                return StatusCode(StatusCodes.Status403Forbidden);

            DistributionList? distributionList = await database.DistributionLists.SingleOrDefaultAsync(l => l.Id == id);

            if (distributionList == null)
                return StatusCode(StatusCodes.Status404NotFound);

            distributionList.Alias = request.Alias;
            distributionList.Flags = request.Newsletter ? DistributionListFlags.Newsletter : DistributionListFlags.None;

            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("~/api/distribution-list/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDistributionList(long id)
        {
            if (!await filterService.HasPermission(User, Permissions.DistributionLists_Admin))
                return StatusCode(StatusCodes.Status403Forbidden);

            DistributionList? distributionList = await database.DistributionLists
                .Include(l => l.PermittedRecipients)
                .Include(l => l.PermittedSenders)
                .SingleOrDefaultAsync(l => l.Id == id);

            if (distributionList == null)
                return StatusCode(StatusCodes.Status404NotFound);

            database.DistributionLists.Remove(distributionList);

            if (distributionList.PermittedSenders != null)
                database.PersonFilterLists.Remove(distributionList.PermittedSenders);

            if (distributionList.PermittedRecipients != null)
                database.PersonFilterLists.Remove(distributionList.PermittedRecipients);

            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
