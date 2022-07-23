namespace Korga.Server.EmailRelay.Entities;

public class EmailRecipient
{
    public EmailRecipient(string emailAddress)
    {
        EmailAddress = emailAddress;
    }

    public long Id { get; set; }
    public string EmailAddress { get; set; }

    public long EmailId { get; set; }
    public Email? Email { get; set; }
}
