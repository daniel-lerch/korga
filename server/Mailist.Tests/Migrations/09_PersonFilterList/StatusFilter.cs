namespace Mailist.Tests.Migrations.PersonFilterList;

public class StatusFilter : PersonFilter
{
	public Status? Status { get; set; }
	public int StatusId { get; set; }
}
