﻿namespace Korga.ChurchTools.Entities;

public class GroupType
{
    public GroupType(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}