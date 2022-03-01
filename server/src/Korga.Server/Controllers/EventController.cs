using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Controllers
{
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly DatabaseContext database;

        public EventController(DatabaseContext database)
        {
            this.database = database;
        }

        [HttpGet("~/api/events")]
        public async Task<IActionResult> ListEvents()
        {
            // EF Core cannot translate complex group joins in SQL
            var events = await
                (from e in database.Events
                 join p in database.EventPrograms on e.Id equals p.EventId
                 select new { Event = e, Program = p, Count = p.Participants!.Count() })
                .ToListAsync();

            // Grouping operation is performed client-sided
            return new JsonResult(events
                .GroupBy(x => x.Event)
                .Select(x => new EventResponse(x.Key, x.Select(y => new EventResponse.Program(y.Program, y.Count)).ToList()))
                .ToList());
        }
    }
}
