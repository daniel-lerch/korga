using System;

namespace Mailist.Tests.Migrations.PersonFilterList;

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
