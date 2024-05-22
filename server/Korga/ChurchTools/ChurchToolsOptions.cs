using System.ComponentModel.DataAnnotations;

namespace Korga.ChurchTools;

public class ChurchToolsOptions
{
	// Validation ensures that required values cannot be null
	public bool EnableSync { get; set; }
	[Required] public string Host { get; set; } = null!;
	[Required] public string LoginToken { get; set; } = null!;
	[Range(0.0, 1440.0)] public double SyncIntervalInMinutes { get; set; }
}
