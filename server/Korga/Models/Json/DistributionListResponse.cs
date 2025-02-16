using System.Collections.Generic;

namespace Korga.Models.Json;

public class DistributionListResponse
{
	public required long Id { get; init; }
	public required string Alias { get; init; }
	public required bool Newsletter { get; init; }
	public required IReadOnlyList<PersonFilterResponse> PermittedRecipients { get; init; }
    public required IReadOnlyList<PersonFilterResponse> PermittedSenders { get; init; }
}
