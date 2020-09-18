namespace Korga.Server.Database.Entities
{
    public class MutableEntityBase : EntityBase
    {
        public int Version { get; set; }
    }
}
