namespace Korga.Configuration;

public class OpenIdConnectOptions
{
    public string? Authority { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? RedirectUri { get; set; }
}
