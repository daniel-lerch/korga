namespace Korga.Server.Database.Entities;

public class Event
{
    public Event(string name)
    {
        Name = name;
    }

    public long Id { get; set; }
    public string Name { get; set; }
}
