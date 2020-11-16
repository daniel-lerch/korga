using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Controllers
{
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly DatabaseContext database;

        public PersonController(DatabaseContext database)
        {
            this.database = database;
        }

        [HttpGet("~/api/people")]
        public async Task<IActionResult> Index([FromQuery] int offset = 0, [FromQuery] int count = 50)
        {
            var people = await database.People
                .OrderBy(p => p.FamilyName).ThenBy(p => p.GivenName)
                .Select(p => new PersonResponse(p.Id, p.GivenName, p.FamilyName, p.MailAddress))
                .Skip(offset).Take(count).ToListAsync();

            return new JsonResult(people);
        }

        [HttpGet("~/api/person/{id}")]
        public async Task<IActionResult> GetPerson(int id)
        {
            Person? person = await database.People.Where(p => p.Id == id).Include(p => p.CreatedBy).Include(p => p.DeletedBy).SingleOrDefaultAsync();
            if (person == null) return StatusCode(404);

            var memberships = await GetMemberships(id);
            var snapshots = await GetSnapshots(id);

            return new JsonResult(new PersonResponse2(person, memberships, snapshots));
        }

        [HttpPost("~/api/person/new")]
        public async Task<IActionResult> CreatePerson([FromBody] PersonRequest request)
        {
            Person person = new Person(request.GivenName, request.FamilyName)
            {
                MailAddress = request.MailAddress
            };

            database.People.Add(person);
            await database.SaveChangesAsync();

            return new JsonResult(new PersonResponse2(person, Array.Empty<PersonResponse2.Membership>(), Array.Empty<PersonSnapshot>()));
        }

        [HttpPut("~/api/person/{id}")]
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] PersonRequest request)
        {
            Person? person = await database.People.AsTracking().Where(p => p.Id == id).Include(p => p.CreatedBy).Include(p => p.DeletedBy).SingleOrDefaultAsync();
            if (person == null) return StatusCode(404);

            if (request.Changes(person))
            {
                await database.UpdatePerson(person, p =>
                {
                    p.GivenName = request.GivenName;
                    p.FamilyName = request.FamilyName;
                    p.MailAddress = request.MailAddress;
                    p.Version++;
                });
            }

            var memberships = await GetMemberships(id);
            var snapshots = await GetSnapshots(id);

            return new JsonResult(new PersonResponse2(person, memberships, snapshots));
        }

        private Task<List<PersonResponse2.Membership>> GetMemberships(int personId)
        {
            return
                (from g in database.Groups
                 join r in database.GroupRoles on g.Id equals r.GroupId
                 join m in database.GroupMembers on r.Id equals m.GroupRoleId
                 where m.PersonId == personId
                 select new PersonResponse2.Membership(m, r.Name, g.Id, g.Name))
                 .ToListAsync();
        }

        private Task<List<PersonSnapshot>> GetSnapshots(int personId)
        {
            return database.PersonSnapshots.Where(ps => ps.PersonId == personId).Include(ps => ps.OverriddenBy).ToListAsync();
        }
    }
}
