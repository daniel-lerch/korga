using Korga.Server.Models;

namespace Korga.Server.Database.Entities
{
    public class SendRole : EntityBase
    {
        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }

        public ReviewPermission Permission { get; set; }
    }
}
