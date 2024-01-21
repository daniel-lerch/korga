using System;

namespace Korga.Server.Tests.Migrations.GroupStatus;

public class Group
{
    public required int Id { get; set; }
    public required int GroupTypeId { get; set; }
    public GroupType? GroupType { get; set; }
    public required int GroupStatusId { get; set; }
    public GroupStatus? GroupStatus { get; set; }
    public required string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
