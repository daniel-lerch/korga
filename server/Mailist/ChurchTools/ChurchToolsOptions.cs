using System.ComponentModel.DataAnnotations;

namespace Mailist.ChurchTools;

public class ChurchToolsOptions
{
	// Validation ensures that required values cannot be null
	public bool EnableSync { get; set; }
	[Required] public string Host { get; set; } = null!;
	[Required] public string LoginToken { get; set; } = null!;
}
