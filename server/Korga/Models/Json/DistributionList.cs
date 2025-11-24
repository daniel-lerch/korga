using System.Text.Json;

namespace Korga.Models.Json;

public class CreateDistributionList
{
    public CreateDistributionList(string alias, bool newsletter, JsonElement? recipientsQuery)
    {
        Alias = alias;
        Newsletter = newsletter;
        RecipientsQuery = recipientsQuery;
    }
    public string Alias { get; set; }
    public bool Newsletter { get; set; }
    public JsonElement? RecipientsQuery { get; set; }
}

public class DistributionList : CreateDistributionList
{
	public DistributionList(long id, string alias, bool newsletter, JsonElement? recipientsQuery)
        : base(alias, newsletter, recipientsQuery)
	{
		Id = id;
    }

	public long Id { get; set; }
}
