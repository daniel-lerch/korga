namespace Korga.Tests.Migrations.Permissions;

public class DistributionList
{
    public long Id { get; set; }
    public required string Alias { get; set; }
    public int Flags { get; set; }
    public long? PermittedRecipientsId { get; set; }
    public PersonFilterList? PermittedRecipients { get; set; }
    public long? PermittedSendersId { get; set; }
    public PersonFilterList? PermittedSenders { get; set; }
}
