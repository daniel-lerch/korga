namespace Korga.Server.Models.Json;

public class ServiceResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? ServiceGroupName { get; init; }
}
