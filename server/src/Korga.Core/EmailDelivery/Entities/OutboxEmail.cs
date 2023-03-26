using System;
using Korga.EmailRelay.Entities;

namespace Korga.EmailDelivery.Entities;

public class OutboxEmail
{
    public OutboxEmail(string emailAddress, byte[] content)
    {
        EmailAddress = emailAddress;
        Content = content;
    }

    public long Id { get; set; }

    public long InboxEmailId { get; set; }
    public InboxEmail? InboxEmail { get; set; }

    public string EmailAddress { get; set; }
    public byte[] Content { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeliveryTime { get; set; }
}
