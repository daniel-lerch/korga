using Korga.Server.Models;
using System;

namespace Korga.Server.Database.Entities
{
    public class MessageReview
    {
        public int Id { get; set; }

        public int MessageAssignmentId { get; set; }
        public MessageAssignment? MessageAssignment { get; set; }

        public int PersonId { get; set; }
        public Person? Person { get; set; }

        public ReviewOpinion Opinion { get; set; }
        public DateTime CreationTime { get; set; }
        public string? Comment { get; set; }
    }
}
