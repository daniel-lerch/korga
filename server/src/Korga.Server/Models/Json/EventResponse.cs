using Korga.EventRegistration.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json;

public class EventResponse
{
    [JsonConstructor]
    public EventResponse(string name, IList<Program> programs)
    {
        Name = name;
        Programs = programs;
    }

    public EventResponse(Event @event, IList<Program> programs)
    {
        Id = @event.Id;
        Name = @event.Name;
        Programs = programs;
    }

    public long Id { get; set; }
    public string Name { get; set; }
    public IList<Program> Programs { get; set; }

    public class Program
    {
        [JsonConstructor]
        public Program(string name)
        {
            Name = name;
        }

        public Program(EventProgram program, int count)
        {
            Id = program.Id;
            Name = program.Name;
            Limit = program.Limit;
            Count = count;
        }

        public long Id { set; get; }
        public string Name { set; get; }
        public int Limit { set; get; }
        public int Count { set; get; }
    }
}
