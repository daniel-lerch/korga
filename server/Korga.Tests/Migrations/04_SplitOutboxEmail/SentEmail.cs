using System;

namespace Korga.Tests.Migrations.SplitOutboxEmail;

public class SentEmail
{
    public long Id { get; set; }

    public long? InboxEmailId { get; set; }

    public required string EmailAddress { get; set; }
    public int ContentSize { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeliveryTime { get; set; }
}
