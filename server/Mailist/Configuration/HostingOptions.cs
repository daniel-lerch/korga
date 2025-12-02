namespace Mailist.Configuration;

public class HostingOptions
{
    public string? PathBase { get; set; }
    public bool AllowProxies { get; set; }
    public string[]? CorsOrigins { get; set; }
}
