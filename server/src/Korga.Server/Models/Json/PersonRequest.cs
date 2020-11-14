using Korga.Server.Database.Entities;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json
{
    public class PersonRequest
    {
        [JsonConstructor]
        public PersonRequest(string givenName, string familyName, string? mailAddress)
        {
            GivenName = givenName;
            FamilyName = familyName;
            MailAddress = mailAddress;
        }

        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }

        public bool Changes(Person person)
        {
            return GivenName != person.GivenName
                || FamilyName != person.FamilyName
                || MailAddress != person.MailAddress;
        }
    }
}
