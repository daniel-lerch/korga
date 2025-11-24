using System;

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
    public string? RecipientsQuery { get; set; }
    public int RecipientCount { get; set; }
    public DateTime RecipientCountTime { get; set; }
}
