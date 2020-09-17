namespace Korga.Server.Database.Entities
{
    public class ReceiveRole : EntityBase
    {
        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }
    }
}
