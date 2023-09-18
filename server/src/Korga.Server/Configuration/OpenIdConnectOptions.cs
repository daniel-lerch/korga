using System.ComponentModel.DataAnnotations;

namespace Korga.Server.Configuration;

public class OpenIdConnectOptions
{
    // Validation ensures that required values cannot be null
    [Required] public string Authority { get; set; } = null!;
    [Required] public string ClientId { get; set; } = null!;
    [Required] public string ClientSecret { get; set; } = null!;
}
