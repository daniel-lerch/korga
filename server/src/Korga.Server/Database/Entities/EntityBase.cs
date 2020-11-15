using System;

namespace Korga.Server.Database.Entities
{
    public abstract class EntityBase
    {
        public DateTime CreationTime { get; set; }
        public int? CreatedById { get; set; }
        public Person? CreatedBy { get; set; }

        public DateTime DeletionTime { get; set; }
        public int? DeletedById { get; set; }
        public Person? DeletedBy { get; set; }
    }

    public abstract class MutableEntityBase : EntityBase
    {
        public int Version { get; set; }
    }
}
