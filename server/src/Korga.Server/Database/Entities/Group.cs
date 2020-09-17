namespace Korga.Server.Database.Entities
{
    public class Group : EntityBase
    {
        public Group(string name)
        {
            Name = name;
        }

        public int Id { get; set; }

        public string Name { get; set; }
    }
}
