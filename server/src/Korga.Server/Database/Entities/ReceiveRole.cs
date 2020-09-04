namespace Korga.Server.Database.Entities
{
    public class ReceiveRole
    {
        public GroupRole? GroupRole { get; set; }
        public int GroupRoleId { get; set; }

        public DistributionList? DistributionList { get; set; }
        public int DistributionListId { get; set; }
    }
}
