namespace Mailist.Tests.Migrations.SplitOutboxEmail;

public class Group
{
    public required int Id { get; set; }
    public required int GroupTypeId { get; set; }
    public GroupType? GroupType { get; set; }
    public required string Name { get; set; }
}
