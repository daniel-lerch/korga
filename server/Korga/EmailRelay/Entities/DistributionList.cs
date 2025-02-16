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

    // If PermittedRecipients is null or empty, nobody will receive emails from this list
	public long? PermittedRecipientsId { get; set; }
	public PersonFilterList? PermittedRecipients { get; set; }

    // If PermittedSenders is null or empty, everybody can send emails to this list even unauthenticated users
    public long? PermittedSendersId { get; set; }
    public PersonFilterList? PermittedSenders { get; set; }
}
