using System;

namespace Korga.Server.Tests.Migrations.AddSinglePersonFilter;

public class Email
{
    public long Id { get; set; }

    public long? DistributionListId { get; set; }

    public required string Subject { get; set; }
    public required string From { get; set; }
    public string? Sender { get; set; }
    public required string To { get; set; }
    public string? Receiver { get; set; }
    public required byte[] Body { get; set; }
    public DateTime DownloadTime { get; set; }
    public DateTime RecipientsFetchTime { get; set; }
}
