using ChurchTools.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.EmailRelay;

public class IdNameEmail : IChurchQueryResult
{
    public static string PrimaryEntityAlias => "person";

    public static IEnumerable<string> ResponseFields => ["person.id", "person.firstName", "person.lastName", "person.email"];

    [JsonPropertyName("person__id")]
    public required int Id { get; init; }

    [JsonPropertyName("person__firstName")]
    public required string FirstName { get; init; }

    [JsonPropertyName("person__lastName")]
    public required string LastName { get; init; }

    [JsonPropertyName("person__email")]
    public required string Email { get; init; }
}
