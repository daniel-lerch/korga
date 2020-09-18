namespace Korga.Server.Database.Entities
{
    public class GroupSnapshot : SnapshotBase
    {
        public GroupSnapshot(string name)
        {
            Name = name;
        }

        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string Name { get; set; }
    }
}
