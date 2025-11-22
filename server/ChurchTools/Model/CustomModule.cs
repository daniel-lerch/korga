namespace ChurchTools.Model;

public class CustomModule
{
    public CustomModule(int id, string shorty, string name, string? description, int sortKey)
    {
        Id = id;
        Shorty = shorty;
        Name = name;
        Description = description;
        SortKey = sortKey;
    }

    public int Id { get; }
    public string Shorty { get; }
    public string Name { get; }
    public string? Description { get; }
    public int SortKey { get; }
}
