namespace Korga.Server.Database.Entities
{
    public class GroupMember : EntityBase
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }
    }
}
