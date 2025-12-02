namespace Mailist.Tests.Migrations.GroupMemberStatus;

public class StatusFilter : PersonFilter
{
	public Status? Status { get; set; }
	public int StatusId { get; set; }
}
