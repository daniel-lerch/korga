using System;

namespace Korga.Server.Database.Entities
{
    public class GroupMember
    {
        public int PersonId { get; set; }
        public Person? Person { get; set; }

        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public DateTime AccessionTime { get; set; }
        public DateTime ResignationTime { get; set; }
    }
}
