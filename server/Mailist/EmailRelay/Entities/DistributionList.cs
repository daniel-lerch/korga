using System;

namespace Mailist.EmailRelay.Entities;

public class DistributionList
{
	public DistributionList(string alias, string recipientsQuery)
	{
		Alias = alias;
        RecipientsQuery = recipientsQuery;
    }

	public long Id { get; set; }
	public string Alias { get; set; }
	public DistributionListFlags Flags { get; set; }
    public string RecipientsQuery { get; set; }
    public int RecipientCount { get; set; }
    public DateTime RecipientCountTime { get; set; }
}
