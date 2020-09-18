namespace Korga.Server.Database.Entities
{
    public class SendRole : MutableEntityBase
    {
        public int Id { get; set; }

        public int GroupRoleId { get; set; }
        public GroupRole? GroupRole { get; set; }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }
    }
}
