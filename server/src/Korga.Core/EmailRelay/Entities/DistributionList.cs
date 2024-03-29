﻿using System.Collections.Generic;

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

	public IEnumerable<PersonFilter>? Filters { get; set; }
}
