namespace Korga.ChurchTools.Entities;

public class Group
{
    public Group(int id, int groupTypeId, string name)
    {
        Id = id;
        GroupTypeId = groupTypeId;
        Name = name;
    }

    public int Id { get; set; }

    public GroupType? GroupType { get; set; }
    public int GroupTypeId { get; set; }

    public string Name { get; set; }
}
