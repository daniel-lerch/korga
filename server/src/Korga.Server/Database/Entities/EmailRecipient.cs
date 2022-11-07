using System;

namespace Korga.Server.Database.Entities;

public class EmailRecipient
{
    public EmailRecipient(string emailAddress, string givenName, string familyName)
    {
        EmailAddress = emailAddress;
        GivenName = givenName;
        FamilyName = familyName;
    }

    public long Id { get; set; }

    public long EmailId { get; set; }
    public Email? Email { get; set; }

    public string EmailAddress { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public DateTime DeliveryTime { get; set; }
}
