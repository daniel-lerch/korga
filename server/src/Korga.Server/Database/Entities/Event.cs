using Korga.Server.Models;
using System;

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
    public RegistrationPeriod RegistrationPeriod { get; set; }
}
