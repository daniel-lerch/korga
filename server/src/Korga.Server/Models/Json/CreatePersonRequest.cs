using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json
{
    public class CreatePersonRequest
    {
        [JsonConstructor]
        public CreatePersonRequest(string givenName, string familyName, string? mailAddress)
        {
            GivenName = givenName;
            FamilyName = familyName;
            MailAddress = mailAddress;
        }

        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string? MailAddress { get; set; }
    }
}
