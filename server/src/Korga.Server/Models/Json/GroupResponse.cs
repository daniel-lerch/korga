using System.Collections.Generic;

namespace Korga.Server.Models.Json
{
    public class GroupResponse
    {
        public GroupResponse(int id, string name, string? description, int memberCount)
        {
            Id = id;
            Name = name;
            Description = description;
            Roles = new List<Role>();
            MemberCount = memberCount;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public IList<Role> Roles { get; set; }
        public int MemberCount { get; set; }

        public class Role
        {
            public Role(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
