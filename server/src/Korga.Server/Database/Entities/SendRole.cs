using Korga.Server.Models;

namespace Korga.Server.Database.Entities
{
    public class SendRole : MutableEntityBase
    {
        public int Id { get; set; }

        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }

        public ReviewPermission Permission { get; set; }
    }

    public class SendRoleSnapshot : SnapshotBase
    {
        public int SendRoleId { get; set; }
        public SendRole? SendRole { get; set; }

        public ReviewPermission Permission { get; set; }
    }
}
