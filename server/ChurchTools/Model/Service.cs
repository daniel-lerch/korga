﻿using System.Collections.Generic;

namespace ChurchTools.Model;

public class Service
{
    public Service(int id, string name, int serviceGroupId, int sortKey, List<int>? groupIds, string tagIds)
    {
        Id = id;
        Name = name;
        ServiceGroupId = serviceGroupId;
        SortKey = sortKey;
        GroupIds = groupIds;
        TagIds = tagIds;
    }

    public int Id { get; }
    public string Name { get; }
    public int ServiceGroupId { get; }
    public int SortKey { get; }
    /// <summary>
    /// Comma separated list of standard groups IDs
    /// </summary>
    public List<int>? GroupIds { get; }
    /// <summary>
    /// Comma separated list of person tag IDs
    /// </summary>
    public string? TagIds { get; }
}
