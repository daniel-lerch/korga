using System;

namespace Korga.Server.Tests.Migrations.GroupMemberStatus;

public class Group
{
    public int Id { get; set; }

    public GroupType? GroupType { get; set; }
    public int GroupTypeId { get; set; }
    public GroupStatus? GroupStatus { get; set; }
    public int GroupStatusId { get; set; }

    public required string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
