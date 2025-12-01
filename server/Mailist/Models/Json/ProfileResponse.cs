namespace Mailist.Models.Json;

public class ProfileResponse
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string GivenName { get; init; }
    public required string FamilyName { get; init; }
    public required string EmailAddress { get; init; }
    public string? Picture { get; init; }
}
