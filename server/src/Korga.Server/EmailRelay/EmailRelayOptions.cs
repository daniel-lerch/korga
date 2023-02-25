using System.ComponentModel.DataAnnotations;

namespace Korga.Server.EmailRelay;

public class EmailRelayOptions
{
    // Validation ensures that required values cannot be null
    public bool Enable { get; set; }

    [Required] public string ImapHost { get; set; } = null!;
    [Range(1, 65535)] public int ImapPort { get; set; }
    public bool ImapUseSsl { get; set; }
    [Required] public string ImapUsername { get; set; } = null!;
    [Required] public string ImapPassword { get; set; } = null!;

    [Range(0.0, 1440.0)] public double RetrievalIntervalInMinutes { get; set; }

    [Required] public string SenderName { get; set; } = null!;
    [Required] public string SenderAddress { get; set; } = null!;
    [Required] public string SmtpHost { get; set; } = null!;
    [Range(1, 65535)] public int SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    [Required] public string SmtpUsername { get; set; } = null!;
    [Required] public string SmtpPassword { get; set; } = null!;
}
