using System.ComponentModel.DataAnnotations;

namespace Korga.Configuration;

public class JwtOptions
{
    // Validation ensures that required values cannot be null
    [Required] public string Issuer { get; set; } = null!;
    [Required] public string Audience { get; set; } = null!;
    [Required] public string SigningKey { get; set; } = null!;
}
