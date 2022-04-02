using System.Collections.Generic;

namespace Korga.Server.Database.Entities;

public class EventProgram
{
    public EventProgram(string name)
    {
        Name = name;
    }

    public long Id { get; set; }

    public long EventId { get; set; }
    public Event? Event { get; set; }

    public string Name { get; set; }
    public int Limit { get; set; }

    public IEnumerable<EventParticipant>? Participants { get; set; }
}
