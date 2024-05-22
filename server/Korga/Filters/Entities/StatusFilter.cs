using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class StatusFilter : PersonFilter
{
	public Status? Status { get; set; }
	public int StatusId { get; set; }

    public override PersonFilterEqualityKey GetEqualityKey()
    {
        return new PersonFilterEqualityKey(nameof(StatusFilter), StatusId: StatusId);
    }
}
