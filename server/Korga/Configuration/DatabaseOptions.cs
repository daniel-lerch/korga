using System.ComponentModel.DataAnnotations;

namespace Korga.Configuration;

public class DatabaseOptions
{
    // Validation ensures that required values cannot be null
    [Required] public string ConnectionString { get; set; } = null!;
    public bool MigrateOnStartup { get; set; }
}
