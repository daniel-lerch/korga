namespace Korga.Models.Json;

public class SettingsResponse
{
    public required string OidcAuthority { get; init; }
    public required string OidcClientId { get; init; }
    public required string OidcRedirectUri { get; init; }
}
