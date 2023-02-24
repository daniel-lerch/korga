using System.Collections.Generic;

namespace Korga.Server.Models.Json;

public class DistributionListResponse
{
	public DistributionListResponse(long id, string alias, IReadOnlyList<PersonFilter> filters)
	{
		Id = id;
		Alias = alias;
		Filters = filters;
	}

	public long Id { get; set; }
	public string Alias { get; set; }
	public IReadOnlyList<PersonFilter> Filters { get; set; }

	public class PersonFilter
	{
		public required long Id { get; set; }
		public required string Discriminator { get; set; }
		public string? StatusName { get; set; }
		public string? GroupName { get; set; }
		public string? GroupRoleName { get; set; }
		public string? PersonFullName { get; set; }
	}
}
