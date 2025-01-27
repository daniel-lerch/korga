namespace Korga.Configuration;

public class OAuthOptions
{
    public string? AuthorizationEndpoint { get; set; }
    public string? TokenEndpoint { get; set; }
    public string? UserInfoEndpoint { get; set; }
    public bool UsePkce { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
