using Korga.Server.Extensions;
using System;

namespace Korga.Server.Services;

public class LdapUidService
{
	public string GetUid(string givenName, string familyName)
	{
		string normalizedGivenName = Normalize(givenName.ToLowerInvariant());
		string normalizedFamilyName = Normalize(familyName.ToLowerInvariant());

		return normalizedGivenName.Take(3) + normalizedFamilyName.Take(4);
	}

	private static string Normalize(string input)
	{
		return input
			.Replace(" ", "", StringComparison.Ordinal)
			.Replace("-", "", StringComparison.Ordinal)
			.Replace("ä", "ae", StringComparison.Ordinal)
			.Replace("é", "e", StringComparison.Ordinal)
			.Replace("è", "e", StringComparison.Ordinal)
			.Replace("ë", "e", StringComparison.Ordinal)
			.Replace("ö", "oe", StringComparison.Ordinal)
			.Replace("ü", "ue", StringComparison.Ordinal)
			.Replace("ß", "ss", StringComparison.Ordinal);
	}
}
