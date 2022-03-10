namespace Korga.Server.Database.Entities;

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
    public long RegistrationId { get; set; }
    public EventRegistration? Registration { get; set; }

    public string GivenName { get; set; }
    public string FamilyName { get; set; }
}
