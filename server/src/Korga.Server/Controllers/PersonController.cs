﻿using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DbPerson = Korga.Server.Database.Entities.Person;
using Person = Korga.Server.Models.Json.Person;

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
                .Select(p => new Person(p.Id, p.GivenName, p.FamilyName, p.MailAddress))
                .Skip(offset).Take(count).ToListAsync();

            return new JsonResult(people);
        }

        [HttpGet("~/api/person/{id}")]
        public async Task<IActionResult> GetPerson(int id)
        {
            DbPerson? person = await database.People.Where(p => p.Id == id).Include(p => p.Creator).Include(p => p.Deletor).SingleOrDefaultAsync();
            if (person == null) return StatusCode(404);

            List<PersonSnapshot> snapshots = await database.PersonSnapshots.Where(ps => ps.PersonId == id).Include(ps => ps.Editor).ToListAsync();

            return new JsonResult(new Person2(person, snapshots));
        }
    }
}