namespace Korga.ChurchTools.Api;

public class Service
{
    public Service(int id, string name, int serviceGroupId, int sortKey, string groupIds, string tagIds)
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
    public string GroupIds { get; }
    /// <summary>
    /// Comma separated list of person tag IDs
    /// </summary>
    public string TagIds { get; }
}
