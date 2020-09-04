namespace Korga.Server.Database.Entities
{
    public class DistributionList
    {
        public DistributionList(int id, string alias, string name)
        {
            Id = id;
            Alias = alias;
            Name = name;
        }

        public int Id { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
