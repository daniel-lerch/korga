using System;
using ChurchTools.Model;

namespace Mailist.ChurchTools.Entities;

public class Department : IIdentifiable<int>, IArchivable
{
    public Department(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
