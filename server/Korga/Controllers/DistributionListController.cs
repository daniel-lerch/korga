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

        [Authorize]
        [HttpGet("~/api/distribution-lists")]
        [ProducesResponseType(typeof(DistributionListResponse[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDistributionLists()
        {
            if (!await filterService.HasPermission(User, "distribution-lists:view"))
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
    }
}
