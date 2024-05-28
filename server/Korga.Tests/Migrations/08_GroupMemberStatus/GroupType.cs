using System;

namespace Korga.Tests.Migrations.GroupMemberStatus;

public class GroupType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
