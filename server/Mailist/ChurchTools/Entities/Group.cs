using System;
using ChurchTools.Model;

namespace Mailist.ChurchTools.Entities;

public class Group : IIdentifiable<int>, IArchivable
{
    public Group(int id, int groupTypeId, int groupStatusId, string name)
    {
        Id = id;
        GroupTypeId = groupTypeId;
        GroupStatusId = groupStatusId;
        Name = name;
    }

    public int Id { get; set; }

    public GroupType? GroupType { get; set; }
    public int GroupTypeId { get; set; }
    public GroupStatus? GroupStatus { get; set; }
    public int GroupStatusId { get; set; }

    public string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
