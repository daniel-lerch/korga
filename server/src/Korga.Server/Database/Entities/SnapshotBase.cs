using System;

namespace Korga.Server.Database.Entities
{
    public class SnapshotBase
    {
        public int Version { get; set; }

        public DateTime EditTime { get; set; }
        public int? EditorId { get; set; }
        public Person? Editor { get; set; }
    }
}
