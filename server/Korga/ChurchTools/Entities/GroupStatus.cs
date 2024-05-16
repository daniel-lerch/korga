using System;
using ChurchTools.Model;

namespace Korga.ChurchTools.Entities;

public class GroupStatus : IIdentifiable<int>, IArchivable
{
    public GroupStatus(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
