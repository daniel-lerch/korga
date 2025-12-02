namespace Mailist.Tests.Migrations.SplitOutboxEmail;

public class OutboxEmail
{
    public long Id { get; set; }

    public long? InboxEmailId { get; set; }

    public required string EmailAddress { get; set; }
    public required byte[] Content { get; set; }
}
