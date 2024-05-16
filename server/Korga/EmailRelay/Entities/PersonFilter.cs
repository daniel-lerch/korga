namespace Korga.EmailRelay.Entities;

public abstract class PersonFilter
{
    public long Id { get; set; }

    public long DistributionListId { get; set; }
    public DistributionList? DistributionList { get; set; }
}
