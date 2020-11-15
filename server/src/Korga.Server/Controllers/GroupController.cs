using Korga.Server.Database;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Controllers
{
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly DatabaseContext database;

        public GroupController(DatabaseContext database)
        {
            this.database = database;
        }

        [HttpGet("~/api/groups")]
        public async Task<IActionResult> Index()
        {
            var groups = await
                (from g in database.Groups
                 join r in database.GroupRoles on g.Id equals r.GroupId into grouping
                 from r in grouping.DefaultIfEmpty()
                 select new { g.Id, g.Name, g.Description, RoleId = r.Id, RoleName = r.Name })
            .ToListAsync();

            // SQL:
            // SELECT g.Id groupId, COUNT(DISTINCT m.PersonId)
            // FROM groups g 
            // INNER JOIN grouproles r ON g.Id = r.GroupId
            // INNER JOIN groupmembers m ON r.Id = m.GroupRoleId
            // GROUP BY g.Id;

            var members = await
                (from g in database.Groups
                 join r in database.GroupRoles on g.Id equals r.GroupId
                 join m in database.GroupMembers on r.Id equals m.GroupRoleId
                 group new { GroupId = g.Id, m.PersonId } by g.Id into grouping
                 select new { GroupId = grouping.Key, MemberCount = grouping.Select(x => x.PersonId).Distinct().Count() })
            .ToDictionaryAsync(x => x.GroupId, x => x.MemberCount);

            var result = new List<GroupResponse>();
            GroupResponse? principal = null;

            foreach (var group in groups)
            {
                if (principal?.Id != group.Id)
                {
                    principal = new GroupResponse(group.Id, group.Name, group.Description, members[group.Id]);
                    result.Add(principal);
                }

                if (group.RoleId != default)
                {
                    principal.Roles.Add(new GroupResponse.Role(group.RoleId, group.RoleName));
                }
            }

            return new JsonResult(result);
        }
    }
}
