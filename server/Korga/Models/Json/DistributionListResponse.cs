namespace Korga.Models.Json;

public class DistributionListResponse
{
	public DistributionListResponse(long id, string alias, bool newsletter, string? recipientsQuery)
	{
		Id = id;
		Alias = alias;
		Newsletter = newsletter;
        RecipientsQuery = recipientsQuery;
    }

	public long Id { get; set; }
	public string Alias { get; set; }
	public bool Newsletter { get; set; }
    public string? RecipientsQuery { get; set; }
}
