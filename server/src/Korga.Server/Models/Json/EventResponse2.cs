using Korga.Server.Database.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json;

public class EventResponse2
{
    [JsonConstructor]
    public EventResponse2(string name, IList<Program> programs)
    {
        Name = name;
        Programs = programs;
    }

    public EventResponse2(Event @event, IList<Program> programs)
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
        public Program(string name, IList<Participant> participants)
        {
            Name = name;
            Participants = participants;
        }

        public Program(EventProgram program, IList<Participant> participants)
        {
            Id = program.Id;
            Name = program.Name;
            Limit = program.Limit;
            Participants = participants;
        }

        public long Id { set; get; }
        public string Name { set; get; }
        public int Limit { set; get; }
        public IList<Participant> Participants { set; get; }
    }

    public class Participant
    {
        [JsonConstructor]
        public Participant(string givenName, string familyName)
        {
            GivenName = givenName;
            FamilyName = familyName;
        }

        public Participant(EventParticipant participant)
        {
            Id = participant.Id;
            GivenName = participant.GivenName;
            FamilyName = participant.FamilyName;
        }

        public long Id { set; get; }
        public string GivenName { set; get; }
        public string FamilyName { set; get; }
    }
}
