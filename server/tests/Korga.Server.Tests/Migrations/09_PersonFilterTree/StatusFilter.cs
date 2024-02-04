namespace Korga.Server.Tests.Migrations.PersonFilterTree;

public class StatusFilter : PersonFilter
{
	public Status? Status { get; set; }
	public int StatusId { get; set; }
}
