using System;

namespace Mailist.Tests.Migrations.GroupStatus;

public class GroupStatus
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
