using Korga.Server.Database;
using Korga.Server.Models.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Korga.Server.Services
{
    public class EventRegistrationService
    {
        private readonly DatabaseContext database;

        public EventRegistrationService(DatabaseContext database)
        {
            this.database = database;
        }

        public async Task<int> ValidateRequest(long eventId, EventRegistrationRequest[] request)
        {
            var requestsByProgram = request
                .GroupBy(r => r.ProgramId)
                .Select(g => new { ProgramId = g.Key, Count = g.Count() });

            foreach (var groupedRequests in requestsByProgram)
            {
                var program = await database.EventPrograms
                    .Where(p => p.EventId == eventId && p.Id == groupedRequests.ProgramId)
                    .Select(p => new { Program = p, Count = p.Participants.Count() })
                    .SingleOrDefaultAsync();
                
                // A requested program or the specified event was not found
                if (program is null) return StatusCodes.Status404NotFound;

                // The program has already reached the maximum participants
                if (program.Count + groupedRequests.Count > program.Program.Limit) return StatusCodes.Status409Conflict;
            }

            return StatusCodes.Status200OK;
        }
    }
}
