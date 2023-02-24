using Korga.EmailRelay.Entities;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Controllers
{
	[ApiController]
	public class DistributionListController : ControllerBase
	{
		private readonly DatabaseContext database;

		public DistributionListController(DatabaseContext database)
		{
			this.database = database;
		}

		[HttpGet("~/api/distribution-lists")]
		[ProducesResponseType(typeof(DistributionListResponse[]), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetDistributionLists()
		{
			List<DistributionList> distributionLists = await database.DistributionLists.Include(dl => dl.Filters).OrderBy(dl => dl.Alias).ToListAsync();

			List<DistributionListResponse> response = new();

			foreach (DistributionList distributionList in distributionLists)
			{
				List<DistributionListResponse.PersonFilter> filters = new();

				foreach (PersonFilter personFilter in distributionList.Filters!)
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
					else if (personFilter is SinglePerson singlePerson)
					{
						filter.PersonFullName = await database.People.Where(p => p.Id == singlePerson.PersonId).Select(p => $"{p.FirstName} {p.LastName}").SingleAsync();
					}

					filters.Add(filter);
				}

				response.Add(new(distributionList.Id, distributionList.Alias, filters));
			}

			return new JsonResult(response);
		}
	}
}
