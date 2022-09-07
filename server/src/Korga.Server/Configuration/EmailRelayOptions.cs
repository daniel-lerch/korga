using System.ComponentModel.DataAnnotations;

namespace Korga.Server.Configuration;

public class EmailRelayOptions
{
    // Validation ensures that required values cannot be null
    [Required] public string ChurchToolsApiUrl { get; set; } = null!;
    [Required] public string ChurchToolsLoginToken { get; set; } = null!;
    [Required] public string ChurchToolsEmailAliasGroupField { get; set; } = null!;

    [Required] public string ImapHost { get; set; } = null!;
    [Range(1, 65535)] public int ImapPort { get; set; }
    public bool ImapUseSsl { get; set; }
    [Required] public string ImapUsername { get; set; } = null!;
    [Required] public string ImapPassword { get; set; } = null!;

    [Range(0.0, 1440.0)] public double RetrievalIntervalInMinutes { get; set; }
}
