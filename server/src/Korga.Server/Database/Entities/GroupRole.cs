namespace Korga.Server.Database.Entities
{
    public class GroupRole : MutableEntityBase
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
