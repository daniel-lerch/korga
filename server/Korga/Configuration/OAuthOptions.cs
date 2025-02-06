using System.ComponentModel.DataAnnotations;

namespace Korga.Configuration;

public class OAuthOptions
{
    // Validation ensures that required values cannot be null
    [Required] public string AuthorizationEndpoint { get; set; } = null!;
    [Required] public string TokenEndpoint { get; set; } = null!;
    [Required] public string UserInformationEndpoint { get; set; } = null!;
    public bool UsePkce { get; set; }
    [Required] public string ClientId { get; set; } = null!;
    [Required] public string ClientSecret { get; set; } = null!;
}
