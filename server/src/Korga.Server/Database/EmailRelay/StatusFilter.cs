using Korga.Server.Database.ChurchTools;

namespace Korga.Server.Database.EmailRelay;

public class StatusFilter : PersonFilter
{

}

public class StatusFilterStatus
{
	public StatusFilter? StatusFilter { get; set; }
	public long StatusFilterId { get; set; }

	public Status? Status { get; set; }
	public int StatusId { get; set; }
}
