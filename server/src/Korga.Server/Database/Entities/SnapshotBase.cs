using System;

namespace Korga.Server.Database.Entities
{
    public class SnapshotBase
    {
        public int Version { get; set; }

        public DateTime CreationTime { get; set; }
        public int? CreatorId { get; set; }
        public Person? Creator { get; set; }
    }
}
