namespace Korga.Server.Database.Entities
{
    public class GroupRole : MutableEntityBase
    {
        public GroupRole(string name)
        {
            Name = name;
        }

        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string Name { get; set; }
    }

    public class GroupRoleSnapshot : SnapshotBase
    {
        public GroupRoleSnapshot(string name)
        {
            Name = name;
        }

        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public string Name { get; set; }
    }
}
