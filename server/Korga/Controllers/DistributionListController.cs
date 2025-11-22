using Korga.EmailRelay;
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

		public DistributionListController(DatabaseContext database)
		{
			this.database = database;
		}

		[Authorize]
		[HttpGet("~/api/distribution-lists")]
		[ProducesResponseType(typeof(DistributionListResponse[]), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetDistributionLists()
		{
			List<DistributionListResponse> response = await database.DistributionLists
				.OrderBy(dl => dl.Alias)
                .Select(dl => new DistributionListResponse(
                    dl.Id,
                    dl.Alias,
                    dl.Flags.HasFlag(DistributionListFlags.Newsletter),
                    dl.RecipientsQuery))
				.ToListAsync();

			return new JsonResult(response);
		}
	}
}
