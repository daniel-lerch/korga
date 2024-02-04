using System;

namespace Korga.Server.Tests.Migrations.GroupMemberStatus;

public class Status
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
