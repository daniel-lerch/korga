using System;
using System.Collections.Generic;

namespace Korga.Server.Database.Entities;

public class EventRegistration
{
    public long Id { get; set; }
    public Guid Token { get; set; }

    public IEnumerable<EventParticipant>? Participants { get; set; }
}
