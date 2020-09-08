using System;

namespace Korga.Server.Database.Entities
{
    public class GroupMember
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public DateTime AccessionTime { get; set; }
        public int? AccessorId { get; set; }
        public Person? Accessor { get; set; }

        public DateTime ResignationTime { get; set; }
        public int? ResignatorId { get; set; }
        public Person? Resignator { get; set; }
    }
}
