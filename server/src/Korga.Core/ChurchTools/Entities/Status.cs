using System;

namespace Korga.ChurchTools.Entities;

public class Status : IIdentifiable<int>, IArchivable
{
    public Status(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
