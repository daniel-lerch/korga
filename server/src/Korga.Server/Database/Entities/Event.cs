using System;
using System.Collections.Generic;

namespace Korga.Server.Database.Entities;

public class Event
{
    public Event(string name)
    {
        Name = name;
    }

    public long Id { get; set; }

    public string Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public DateTime RegistrationStart { get; set; }
    public DateTime RegistrationDeadline { get; set; }

    public IReadOnlyList<EventProgram>? Programs { get; set; }
}
