﻿using System.Collections.Generic;

namespace Korga.Models.Json;

public class DistributionListResponse
{
	public DistributionListResponse(long id, string alias, bool newsletter, IReadOnlyList<PersonFilter> permittedRecipients)
	{
		Id = id;
		Alias = alias;
		Newsletter = newsletter;
		PermittedRecipients = permittedRecipients;
	}

	public long Id { get; set; }
	public string Alias { get; set; }
	public bool Newsletter { get; set; }
	public IReadOnlyList<PersonFilter> PermittedRecipients { get; set; }

	public class PersonFilter
	{
		public required long Id { get; set; }
		public required string Discriminator { get; set; }
		public string? StatusName { get; set; }
		public string? GroupName { get; set; }
		public string? GroupTypeName { get; set; }
		public string? GroupRoleName { get; set; }
		public string? PersonFullName { get; set; }
	}
}
