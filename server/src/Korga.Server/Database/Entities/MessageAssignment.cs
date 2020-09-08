using System;

namespace Korga.Server.Database.Entities
{
    public class MessageAssignment
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public Message? Message { get; set; }

        public int DistributionListId { get; set; }
        public DistributionList? DistributionList { get; set; }

        public DateTime CreationTime { get; set; }
        public int? CreatorId { get; set; }
        public Person? Creator { get; set; }

        public DateTime DeletionTime { get; set; }
        public int? DeletorId { get; set; }
        public Person? Deletor { get; set; }
    }
}
