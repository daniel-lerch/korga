namespace Korga.Server.Database.Entities
{
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
