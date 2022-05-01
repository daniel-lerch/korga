using Korga.Server.Database;
using Korga.Server.Database.Entities;
using Korga.Server.Models.Json;
using Korga.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Controllers
{
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventRegistrationService registrationService;
        private readonly DatabaseContext database;

        public EventController(EventRegistrationService registrationService, DatabaseContext database)
        {
            this.registrationService = registrationService;
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
                    .OrderBy(y => y.Id)
                    .ToList()))
                .OrderBy(x => x.Id)
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
                    .OrderByDescending(y => y.Id)
                    .ToList()))
                .OrderBy(x => x.Id)
                .ToList()));
        }

        [HttpGet("~/api/event/{id}/participants/query")]
        [ProducesResponseType(typeof(EventParticipantQueryResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> QueryParticipants(long id, [FromQuery] string givenName, [FromQuery] string familyName)
        {
            database.EventPrograms.Where(p => p.EventId == id);
            var results = await
                (from p in database.EventPrograms
                 where p.EventId == id
                 join pa in database.EventParticipants
                     // String equality is translated to SQL equality which is case-insensitive on MySQL per default
                     .Where(p => p.GivenName == givenName && p.FamilyName == familyName)
                     on p.Id equals pa.ProgramId into grouping
                 from pa in grouping.DefaultIfEmpty()
                 select new { ProgramId = p.Id, pa.GivenName, pa.FamilyName })
                .ToListAsync();

            if (results.Count == 0) return StatusCode(StatusCodes.Status404NotFound);

            return new JsonResult(results
                .Where(p => p.GivenName is not null && p.FamilyName is not null)
                .Select(p => new EventParticipantQueryResponse(p.ProgramId, p.GivenName, p.FamilyName))
                .ToArray());
        }

        [HttpPost("~/api/event/{id}/register")]
        [ProducesResponseType(typeof(EventRegistrationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(long id, [FromBody] EventRegistrationRequest[] request)
        {
            if (!ModelState.IsValid) return StatusCode(StatusCodes.Status400BadRequest);

            // Validate event programs and limits
            int status = await registrationService.ValidateRequest(id, request);
            if (status != StatusCodes.Status200OK) return StatusCode(status);

            // Perform actual registration
            EventRegistration registration = await registrationService.CreateRegistration(id, request);

            return new JsonResult(new EventRegistrationResponse { Id = registration.Id, Token = registration.Token });
        }

        [HttpDelete("~/api/events/registration")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRegistration([FromQuery] Guid token)
        {
            EventRegistration? registration = await database.EventRegistrations.SingleOrDefaultAsync(r => r.Token == token);
            if (registration is null) return StatusCode(StatusCodes.Status404NotFound);

            database.EventRegistrations.Remove(registration);
            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("~/api/events/registration/add")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddParticipants([FromQuery] Guid token, [FromBody] EventRegistrationRequest[] request)
        {
            if (!ModelState.IsValid) return StatusCode(StatusCodes.Status400BadRequest);

            EventRegistration? registration = await database.EventRegistrations.SingleOrDefaultAsync(r => r.Token == token);
            if (registration is null) return StatusCode(StatusCodes.Status404NotFound);

            // Validate event programs and limits
            int status = await registrationService.ValidateRequest(registration.EventId, request);
            if (status != StatusCodes.Status200OK) return StatusCode(status);

            database.EventParticipants.AddRange(request
                .Select(p => new EventParticipant(p.GivenName, p.FamilyName) { ProgramId = p.ProgramId, RegistrationId = registration.Id })
                .ToArray());
            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpDelete("~/api/events/registration/{participantId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteParticipant([FromQuery] Guid token, long participantId)
        {
            var registrationParticipants = await
                (from r in database.EventRegistrations
                 where r.Token == token
                 join p in database.EventParticipants
                 on r.Id equals p.RegistrationId
                 select new { Registration = r, Participant = p })
                .ToListAsync();

            if (registrationParticipants.Count == 0) return StatusCode(StatusCodes.Status404NotFound);

            // Make sure participant ID is in this registration
            var selected = registrationParticipants.SingleOrDefault(x => x.Participant.Id == participantId);
            if (selected is null) return StatusCode(StatusCodes.Status404NotFound);

            if (registrationParticipants.Count == 1)
            {
                // Manual orphan delete when the last participant of a registration is deleted
                database.EventRegistrations.Remove(selected.Registration);
            }
            else
            {
                // There are remaining participants so we remove just this one
                database.EventParticipants.Remove(selected.Participant);
            }
            await database.SaveChangesAsync();

            return StatusCode(StatusCodes.Status204NoContent);
        }

        //[HttpPost("~/api/events/registration/{id}/add")]
        //public async Task<IActionResult> AddParticipant(long id)
        //{
        //    return StatusCode(StatusCodes.Status401Unauthorized);
        //}
    }
}
