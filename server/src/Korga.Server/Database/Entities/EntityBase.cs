using System;

namespace Korga.Server.Database.Entities
{
    public abstract class EntityBase
    {
        public DateTime CreationTime { get; set; }
        public int? CreatorId { get; set; }
        public Person? Creator { get; set; }

        public DateTime DeletionTime { get; set; }
        public int? DeletorId { get; set; }
        public Person? Deletor { get; set; }
    }

    public abstract class MutableEntityBase : EntityBase
    {
        public int Version { get; set; }
    }
}
