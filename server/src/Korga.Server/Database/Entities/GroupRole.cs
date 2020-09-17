namespace Korga.Server.Database.Entities
{
    public class GroupRole : EntityBase
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
}
