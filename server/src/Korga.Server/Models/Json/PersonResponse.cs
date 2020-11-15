using Korga.Server.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json
{
    public class PersonResponse
    {
        [JsonConstructor]
        public PersonResponse(int id, string givenName, string familyName, string? mailAddress)
        {
            Id = id;
            GivenName = givenName;
            FamilyName = familyName;
            MailAddress = mailAddress;
        }

        public PersonResponse(Person person) : this(person.Id, person.GivenName, person.FamilyName, person.MailAddress) { }

        public int Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
    }

    public class PersonResponse2
    {
        [JsonConstructor]
        public PersonResponse2(string givenName, string familyName, IList<Snapshot> history)
        {
            GivenName = givenName;
            FamilyName = familyName;
            History = history;
        }

        public PersonResponse2(Person person, IList<PersonSnapshot> history)
        {
            Id = person.Id;
            Version = person.Version;
            GivenName = person.GivenName;
            FamilyName = person.FamilyName;
            MailAddress = person.MailAddress;
            CreationTime = person.CreationTime;
            CreatedBy = person.CreatedBy == null ? null : new PersonResponse(person.CreatedBy);
            DeletionTime = person.DeletionTime == default ? null : (DateTime?)person.DeletionTime;
            DeletedBy = person.DeletedBy == null ? null : new PersonResponse(person.DeletedBy);
            History = new List<Snapshot>(history.Select(ps => new Snapshot(ps)));
        }

        public int Id { get; set; }
        public int Version { get; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
        public DateTime CreationTime { get; set; }
        public PersonResponse? CreatedBy { get; set; }
        public DateTime? DeletionTime { get; set; }
        public PersonResponse? DeletedBy { get; set; }
        public IList<Snapshot> History { get;set; }

        public class Snapshot
        {
            [JsonConstructor]
            public Snapshot(string givenName, string familyName)
            {
                GivenName = givenName;
                FamilyName = familyName;
            }

            public Snapshot(PersonSnapshot snapshot)
            {
                Version = snapshot.Version;
                GivenName = snapshot.GivenName;
                FamilyName = snapshot.FamilyName;
                MailAddress = snapshot.MailAddress;
                OverrideTime = snapshot.OverrideTime;
                OverriddenBy = snapshot.OverriddenBy is null ? null : new PersonResponse(snapshot.OverriddenBy);
            }

            public int Version { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string? MailAddress { get; set; }
            public DateTime OverrideTime { get; set; }
            public PersonResponse? OverriddenBy { get; set; }
        }
    }
}
