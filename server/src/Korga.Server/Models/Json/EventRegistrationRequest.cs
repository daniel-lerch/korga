using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json;

public class EventRegistrationRequest
{
    [JsonConstructor]
    public EventRegistrationRequest(string givenName, string familyName)
    {
        GivenName = givenName;
        FamilyName = familyName;
    }

    public long ProgramId { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
}
