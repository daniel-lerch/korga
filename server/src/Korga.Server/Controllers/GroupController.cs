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
            // Inner query:
            // SELECT g.Id groupId, COUNT(DISTINCT m.PersonId)
            // FROM groups g 
            // INNER JOIN grouproles r ON g.Id = r.GroupId
            // INNER JOIN groupmembers m ON r.Id = m.GroupRoleId
            // GROUP BY g.Id;

            // I have not yet found a way to omit group by id, name, description with LINQ

            var groups =
                from g in database.Groups
                join r in database.GroupRoles on g.Id equals r.GroupId
                join m in database.GroupMembers on r.Id equals m.GroupRoleId
                group new { Group = g, m.PersonId } by new { g.Id, g.Name, g.Description } into grouping
                select new
                {
                    grouping.Key.Id,
                    grouping.Key.Name,
                    grouping.Key.Description,
                    MemberCount = grouping.Select(x => x.PersonId).Distinct().Count()
                };

            var groupsAndRoles = await
                (from g in groups
                 join r in database.GroupRoles on g.Id equals r.GroupId into grouping
                 from r in grouping.DefaultIfEmpty()
                 select new { g.Id, g.Name, g.Description, g.MemberCount, RoleId = r.Id, RoleName = r.Name })
                 .ToListAsync();

            var result = new List<GroupResponse>();
            GroupResponse? principal = null;

            foreach (var group in groupsAndRoles)
            {
                if (principal?.Id != group.Id)
                {
                    principal = new GroupResponse(group.Id, group.Name, group.Description, group.MemberCount);
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
