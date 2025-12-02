using Mailist.EmailRelay.Entities;
using System;

namespace Mailist.EmailDelivery.Entities;

public class SentEmail
{
    public long Id { get; set; }

    public long? InboxEmailId { get; set; }
    public InboxEmail? InboxEmail { get; set; }

    public required string EmailAddress { get; set; }
    public int ContentSize { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeliveryTime { get; set; }
}
