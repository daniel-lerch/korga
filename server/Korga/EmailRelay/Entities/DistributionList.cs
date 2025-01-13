using Korga.Filters.Entities;

namespace Korga.EmailRelay.Entities;

public class DistributionList
{
	public DistributionList(string alias)
	{
		Alias = alias;
	}

	public long Id { get; set; }
	public string Alias { get; set; }
	public DistributionListFlags Flags { get; set; }

	public long? PermittedRecipientsId { get; set; }
	public PersonFilterList? PermittedRecipients { get; set; }
}
