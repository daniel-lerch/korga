namespace Korga.ChurchTools.Entities;

public class Department : IIdentifiable<int>
{
    public Department(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }
}
