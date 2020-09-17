namespace Korga.Server.Database.Entities
{
    public class DistributionList : EntityBase
    {
        public DistributionList(string alias, string name)
        {
            Alias = alias;
            Name = name;
        }

        public int Id { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
