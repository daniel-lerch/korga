namespace Korga.Models.Json;

public class PersonFilterResponse
{
    public required long Id { get; init; }
    public required string Discriminator { get; init; }
    public string? StatusName { get; set; }
    public string? GroupName { get; set; }
    public string? GroupTypeName { get; set; }
    public string? GroupRoleName { get; set; }
    public string? PersonFullName { get; set; }
}
