namespace Korga.Tests.Migrations.GroupMemberStatus;

public class DistributionList
{
    public required long Id { get; set; }
    public required string Alias { get; set; }
    public int Flags { get; set; }
}
