using System;

namespace Korga.Server.Database.Entities
{
    public class GroupRole
    {
        public GroupRole(int id, int groupId, string name)
        {
            Id = id;
            GroupId = groupId;
            Name = name;
        }

        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group? Group { get; set; }

        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime DeletionTime { get; set; }
    }
}
