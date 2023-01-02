namespace Korga.Server.Database.EventRegistration;

public class EventParticipant
{
    public EventParticipant(string givenName, string familyName)
    {
        GivenName = givenName;
        FamilyName = familyName;
    }

    public long Id { get; set; }

    public long ProgramId { get; set; }
    public EventProgram? Program { get; set; }

    public string GivenName { get; set; }
    public string FamilyName { get; set; }
}
