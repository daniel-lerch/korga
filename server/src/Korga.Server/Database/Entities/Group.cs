using System;

namespace Korga.Server.Database.Entities
{
    public class Group
    {
        public Group(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime DeletionTime { get; set; }
    }
}
