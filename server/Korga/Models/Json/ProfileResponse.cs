using Korga.Filters;
using System.Collections.Generic;

namespace Korga.Models.Json;

public class ProfileResponse
{
    public required string Id { get; init; }
    public required string GivenName { get; init; }
    public required string FamilyName { get; init; }
    public required string EmailAddress { get; init; }
    public Dictionary<Permissions, bool> Permissions { get; init; } = [];
}
