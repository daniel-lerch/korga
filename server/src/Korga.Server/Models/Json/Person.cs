using Korga.Server.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

using DbPerson = Korga.Server.Database.Entities.Person;

namespace Korga.Server.Models.Json
{
    public class Person
    {
        public Person(int id, string givenName, string familyName, string? mailAddress)
        {
            Id = id;
            GivenName = givenName;
            FamilyName = familyName;
            MailAddress = mailAddress;
        }

        public Person(DbPerson person) : this(person.Id, person.GivenName, person.FamilyName, person.MailAddress) { }

        public int Id { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
    }

    public class Person2
    {
        public Person2(DbPerson person, IList<PersonSnapshot> history)
        {
            Id = person.Id;
            Version = person.Version;
            GivenName = person.GivenName;
            FamilyName = person.FamilyName;
            MailAddress = person.MailAddress;
            CreationTime = person.CreationTime;
            Creator = person.Creator == null ? null : new Person(person.Creator);
            DeletionTime = person.DeletionTime == default ? null : (DateTime?)person.DeletionTime;
            Deletor = person.Deletor == null ? null : new Person(person.Deletor);
            History = new List<Snapshot>(history.Select(ps => new Snapshot(ps)));
        }

        public int Id { get; set; }
        public int Version { get; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
        public DateTime CreationTime { get; set; }
        public Person? Creator { get; set; }
        public DateTime? DeletionTime { get; set; }
        public Person? Deletor { get; set; }
        public IList<Snapshot> History { get;set; }

        public class Snapshot
        {
            public Snapshot(PersonSnapshot snapshot)
            {
                Version = snapshot.Version;
                GivenName = snapshot.GivenName;
                FamilyName = snapshot.FamilyName;
                MailAddress = snapshot.MailAddress;
                EditTime = snapshot.EditTime;
                Editor = snapshot.Editor == null ? null : new Person(snapshot.Editor);
            }

            public int Version { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string? MailAddress { get; set; }
            public DateTime EditTime { get; set; }
            public Person? Editor { get; set; }
        }
    }
}
