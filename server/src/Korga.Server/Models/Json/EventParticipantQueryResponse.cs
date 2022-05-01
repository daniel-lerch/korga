using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json
{
    public class EventParticipantQueryResponse
    {
        [JsonConstructor]
        public EventParticipantQueryResponse(long programId, string givenName, string familyName)
        {
            ProgramId = programId;
            GivenName = givenName;
            FamilyName = familyName;
        }

        public long ProgramId { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }
}
