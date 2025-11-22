namespace ChurchTools.Model;

public class CustomModuleDataCategory
{
    public CustomModuleDataCategory(int id, string shorty, string name, string description, string? data)
    {
        Id = id;
        Shorty = shorty;
        Name = name;
        Description = description;
        Data = data;
    }

    public int Id { get; }
    public string Shorty { get; }
    public string Name { get; }
    public string Description { get; }
    public string? Data { get; }
}
