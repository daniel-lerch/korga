using System;
using System.Text.Json;

namespace Korga.Models.Json;

public class CreateDistributionList
{
    public required string Alias { get; init; }
    public required bool Newsletter { get; init; }
    public JsonElement? RecipientsQuery { get; init; }
}

public class DistributionList : CreateDistributionList
{
	public required long Id { get; init; }
    public int RecipientCount { get; init; }
    public DateTime RecipientCountTime { get; init; }
}
