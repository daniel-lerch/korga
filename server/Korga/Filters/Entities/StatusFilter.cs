using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class StatusFilter : PersonFilter
{
	public Status? Status { get; set; }
	public int StatusId { get; set; }

    public override bool FilterConditionEquals(PersonFilter other)
    {
        return other is StatusFilter o && StatusId == o.StatusId;
    }
}
