using Mailist.EmailRelay;
using Mailist.EmailRelay.Entities;
using Mailist.Filters.Entities;
using Mailist.Models.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mailist.Controllers
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
			List<DistributionList> distributionLists = await database.DistributionLists
				.Include(dl => dl.PermittedRecipients)
				.ThenInclude(fl => fl!.Filters)
				.OrderBy(dl => dl.Alias)
				.ToListAsync();

			List<DistributionListResponse> response = [];

			foreach (DistributionList distributionList in distributionLists)
			{
				List<DistributionListResponse.PersonFilter> filters = [];

				foreach (PersonFilter personFilter in distributionList.PermittedRecipients?.Filters ?? [])
				{
					DistributionListResponse.PersonFilter filter = new() { Id = personFilter.Id, Discriminator = personFilter.GetType().Name };

					if (personFilter is StatusFilter statusFilter)
					{
						filter.StatusName = await database.Status.Where(s => s.Id == statusFilter.StatusId).Select(s => s.Name).SingleAsync();
					}
					else if (personFilter is GroupFilter groupFilter)
					{
						filter.GroupName = await database.Groups.Where(g => g.Id == groupFilter.GroupId).Select(g => g.Name).SingleAsync();
						if (groupFilter.GroupRoleId.HasValue)
							filter.GroupRoleName = await database.GroupRoles.Where(r => r.Id == groupFilter.GroupRoleId.Value).Select(r => r.Name).SingleAsync();
					}
					else if (personFilter is GroupTypeFilter groupTypeFilter)
					{
						filter.GroupTypeName = await database.GroupTypes.Where(t => t.Id == groupTypeFilter.GroupTypeId).Select(t => t.Name).SingleAsync();
						if (groupTypeFilter.GroupRoleId.HasValue)
							filter.GroupRoleName = await database.GroupRoles.Where(r => r.Id == groupTypeFilter.GroupRoleId.Value).Select(r => r.Name).SingleAsync();
					}
					else if (personFilter is SinglePerson singlePerson)
					{
						filter.PersonFullName = await database.People.Where(p => p.Id == singlePerson.PersonId).Select(p => $"{p.FirstName} {p.LastName}").SingleAsync();
					}

					filters.Add(filter);
				}

				response.Add(new(distributionList.Id, distributionList.Alias, distributionList.Flags.HasFlag(DistributionListFlags.Newsletter), filters));
			}

			return new JsonResult(response);
		}
	}
}
