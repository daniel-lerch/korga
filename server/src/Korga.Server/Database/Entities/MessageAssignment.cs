namespace Korga.Server.Database.Entities
{
    public class MessageAssignment : EntityBase
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public Message? Message { get; set; }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }
    }
}
