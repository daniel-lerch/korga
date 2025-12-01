using System;

namespace Mailist.Tests.Migrations.ForwardMode;

public class OutboxEmail
{
    public long Id { get; set; }

    public long? InboxEmailId { get; set; }

    public required string EmailAddress { get; set; }
    public required byte[] Content { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime DeliveryTime { get; set; }
}
