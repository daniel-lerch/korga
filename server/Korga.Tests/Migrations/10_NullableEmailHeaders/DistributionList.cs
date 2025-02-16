namespace Korga.Tests.Migrations.NullableEmailHeaders;

public class DistributionList
{
    public long Id { get; set; }
    public required string Alias { get; set; }
    public int Flags { get; set; }
    public long? PermittedRecipientsId { get; set; }
    public PersonFilterList? PermittedRecipients { get; set; }
}
