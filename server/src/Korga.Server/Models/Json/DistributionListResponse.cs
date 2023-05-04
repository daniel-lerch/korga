using System.Collections.Generic;

namespace Korga.Server.Models.Json;

public class DistributionListResponse
{
	public DistributionListResponse(long id, string alias, bool newsletter)
	{
		Id = id;
		Alias = alias;
		Newsletter = newsletter;
	}

	public long Id { get; set; }
	public string Alias { get; set; }
	public bool Newsletter { get; set; }
	public PersonFilter? PermittedRecipients { get; set; }

	public class PersonFilter
	{
		public required long Id { get; set; }
		public required string Discriminator { get; set; }
		public List<PersonFilter> Children { get; set; } = new();
		public string? StatusName { get; set; }
		public string? GroupName { get; set; }
		public string? GroupRoleName { get; set; }
		public string? PersonFullName { get; set; }
	}
}
