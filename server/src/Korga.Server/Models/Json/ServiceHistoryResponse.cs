using Korga.ChurchTools;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json;

public class ServiceHistoryResponse
{
    public required int PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required GroupMemberStatus GroupMemberStatus { get; init; }

    public List<DateOnly> ServiceDates { get; } = new();
}
