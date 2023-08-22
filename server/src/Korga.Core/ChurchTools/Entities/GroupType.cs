﻿using System;

namespace Korga.ChurchTools.Entities;

public class GroupType : IIdentifiable<int>, IArchivable
{
    public GroupType(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
