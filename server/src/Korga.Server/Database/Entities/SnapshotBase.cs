using System;

namespace Korga.Server.Database.Entities
{
    public class SnapshotBase
    {
        public int Version { get; set; }

        public DateTime OverrideTime { get; set; }
        public int? OverriddenById { get; set; }
        public Person? OverriddenBy { get; set; }
    }
}
