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

	public long? PermittedSendersId { get; set; }
	public PersonFilter? PermittedSenders { get; set; }
	public long? PermittedRecipientsId { get; set; }
	public PersonFilter? PermittedRecipients { get; set; }
}
