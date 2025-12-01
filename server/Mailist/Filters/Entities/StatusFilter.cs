using Mailist.ChurchTools.Entities;

namespace Mailist.Filters.Entities;

public class StatusFilter : PersonFilter
{
	public Status? Status { get; set; }
	public int StatusId { get; set; }
}
