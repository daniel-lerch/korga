namespace Korga.ChurchTools.Api;

public class ServiceGroup
{
    public ServiceGroup(int id, string name, int sortKey)
    {
        Id = id;
        Name = name;
        SortKey = sortKey;
    }

    public int Id { get; }
    public string Name { get; }
    public int SortKey { get; }
}
