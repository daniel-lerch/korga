using Korga.Server.Database.Entities;
using Korga.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json
{
    public class PersonResponse
    {
        public static PersonResponse? From(Person? person)
        {
            return person is null ? null : new PersonResponse(person);
        }

        [JsonConstructor]
        public PersonResponse(int id, string givenName, string familyName, string? mailAddress)
        {
            Id = id;
            GivenName = givenName ?? throw new ArgumentNullException(nameof(givenName));
            FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
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
        public PersonResponse2(string givenName, string familyName, IList<Membership> memberships, IList<Snapshot> history)
        {
            GivenName = givenName ?? throw new ArgumentNullException(nameof(givenName));
            FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
            Memberships = memberships ?? throw new ArgumentNullException(nameof(memberships));
            History = history ?? throw new ArgumentNullException(nameof(history));
        }

        public PersonResponse2(Person person, IList<Membership> memberships, IEnumerable<PersonSnapshot> history)
        {
            Id = person.Id;
            Version = person.Version;
            GivenName = person.GivenName;
            FamilyName = person.FamilyName;
            MailAddress = person.MailAddress;
            Memberships = memberships ?? throw new ArgumentNullException(nameof(memberships));
            CreationTime = person.CreationTime;
            CreatedBy = PersonResponse.From(person.CreatedBy);
            DeletionTime = person.DeletionTime.NullIfDefault();
            DeletedBy = PersonResponse.From(person.DeletedBy);
            History = new List<Snapshot>(history.Select(ps => new Snapshot(ps)));
        }

        public int Id { get; set; }
        public int Version { get; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
        public IList<Membership> Memberships { get; set; }
        public DateTime CreationTime { get; set; }
        public PersonResponse? CreatedBy { get; set; }
        public DateTime? DeletionTime { get; set; }
        public PersonResponse? DeletedBy { get; set; }
        public IList<Snapshot> History { get;set; }

        public class Membership
        {
            [JsonConstructor]
            public Membership(string roleName, string groupName)
            {
                RoleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
                GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
            }

            public Membership(GroupMember member, string roleName, int groupId, string groupName)
            {
                Id = member.Id;
                RoleId = member.GroupRoleId;
                RoleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
                GroupId = groupId;
                GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
                CreationTime = member.CreationTime;
                CreatedBy = PersonResponse.From(member.CreatedBy);
                DeletionTime = member.DeletionTime.NullIfDefault();
            }

            public int Id { get; set; }
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public DateTime CreationTime { get; set; }
            public PersonResponse? CreatedBy { get; set; }
            public DateTime? DeletionTime { get; set; }
            public PersonResponse? DeletedBy { get; set; }
        }

        public class Snapshot
        {
            [JsonConstructor]
            public Snapshot(string givenName, string familyName)
            {
                GivenName = givenName ?? throw new ArgumentNullException(nameof(givenName));
                FamilyName = familyName ?? throw new ArgumentNullException(nameof(familyName));
            }

            public Snapshot(PersonSnapshot snapshot)
            {
                Version = snapshot.Version;
                GivenName = snapshot.GivenName;
                FamilyName = snapshot.FamilyName;
                MailAddress = snapshot.MailAddress;
                OverrideTime = snapshot.OverrideTime;
                OverriddenBy = PersonResponse.From(snapshot.OverriddenBy);
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
