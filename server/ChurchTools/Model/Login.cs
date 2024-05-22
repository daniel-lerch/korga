namespace ChurchTools.Model;

public class Login
{
    public Login(int personId, string status, string location, string message)
    {
        PersonId = personId;
        Status = status;
        Location = location;
        Message = message;
    }

    public int PersonId { get; set; }
    public string Status { get; set; }
    public string Location { get; set; }
    public string Message { get; set; }
}
