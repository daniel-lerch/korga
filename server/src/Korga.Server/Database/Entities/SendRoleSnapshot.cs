using Korga.Server.Models;

namespace Korga.Server.Database.Entities
{
    public class SendRoleSnapshot : SnapshotBase
    {
        public int SendRoleId { get; set; }
        public SendRole? SendRole { get; set; }

        public ReviewPermission Permission { get; set; }
    }
}
