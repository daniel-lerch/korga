using System.ComponentModel.DataAnnotations;

namespace Korga.Server.Configuration
{
    public class LdapOptions
    {
        // Validation ensures that required values cannot be null
        [Required] public string Host { get; set; } = null!;
        [Required] public string BindDn { get; set; } = null!;
        [Required] public string BindPassword { get; set; } = null!;
        [Required] public string BaseDn { get; set; } = null!;
    }
}
