using Korga.ChurchTools.Entities;

namespace Korga.EmailRelay.Entities;

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
