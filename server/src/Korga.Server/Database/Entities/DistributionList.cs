namespace Korga.Server.Database.Entities
{
    public class DistributionList : MutableEntityBase
    {
        public DistributionList(string alias)
        {
            Alias = alias;
        }

        public int Id { get; set; }
        public string Alias { get; set; }
    }

    public class DistributionListSnapshot : SnapshotBase
    {
        public DistributionListSnapshot(string alias, string name)
        {
            Alias = alias;
            Name = name;
        }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }

        public string Alias { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
