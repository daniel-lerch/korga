using Korga.Server.Database;
using Korga.Server.Database.Entities;
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
    public class EventController : ControllerBase
    {
        private readonly DatabaseContext database;

        public EventController(DatabaseContext database)
        {
            this.database = database;
        }

        [HttpGet("~/api/events")]
        [ProducesResponseType(typeof(EventResponse[]), StatusCodes.Status200OK)]
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
                .Select(x => new EventResponse(x.Key, x
                    .Select(y => new EventResponse.Program(y.Program, y.Count))
                    .ToList()))
                .ToList());
        }

        [HttpGet("~/api/event/{id}")]
        [ProducesResponseType(typeof(EventResponse2), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(long id)
        {
            Event? @event = await database.Events.SingleOrDefaultAsync(x => x.Id == id);
            if (@event is null) return StatusCode(StatusCodes.Status404NotFound);

            // EF cannot translate simple groups joins either so use a LEFT JOIN
            var programs = await
                (from p in database.EventPrograms
                 where p.EventId == id
                 join pa in database.EventParticipants on p.Id equals pa.ProgramId into grouping
                 from pa in grouping.DefaultIfEmpty()
                 select new { Program = p, Participant = pa })
                .ToListAsync();

            // Grouping operation is performed client-sided respecting null values for programs without participants
            return new JsonResult(new EventResponse2(@event, programs
                .GroupBy(x => x.Program)
                .Select(x => new EventResponse2.Program(x.Key, x
                    .Where(y => y.Participant is not null)
                    .Select(y => new EventResponse2.Participant(y.Participant))
                    .ToList()))
                .ToList()));
        }

        [HttpPost("~/api/events/register")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] EventRegistrationRequest request)
        {
            if (!ModelState.IsValid) return StatusCode(StatusCodes.Status400BadRequest);

            var p = await database.EventPrograms
                .Where(x => x.Id == request.ProgramId)
                .Select(x => new { Program = x, Count = x.Participants!.Count() })
                .SingleOrDefaultAsync();
            if (p is null) return StatusCode(StatusCodes.Status404NotFound);
            if (p.Count >= p.Program.Limit) return StatusCode(StatusCodes.Status409Conflict);

            var participant = new EventParticipant(request.GivenName, request.FamilyName) { ProgramId = request.ProgramId };
            database.EventParticipants.Add(participant);
            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }
        
        [HttpDelete("~/api/events/participant/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRegistration(long id)
        {
            EventParticipant? participant = await database.EventParticipants.SingleOrDefaultAsync(x => x.Id == id);
            if (participant is null) return StatusCode(StatusCodes.Status404NotFound);

            database.EventParticipants.Remove(participant);
            await database.SaveChangesAsync();
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
