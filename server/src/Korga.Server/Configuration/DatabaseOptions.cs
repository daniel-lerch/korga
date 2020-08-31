using System.ComponentModel.DataAnnotations;

namespace Korga.Server.Configuration
{
    public class DatabaseOptions
    {
        // Validation ensures that required values cannot be null
        [Required] public string ConnectionString { get; set; } = null!;
    }
}
