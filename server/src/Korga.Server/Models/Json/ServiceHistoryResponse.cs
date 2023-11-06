using System;
using System.Collections.Generic;

namespace Korga.Server.Models.Json;

public class ServiceHistoryResponse
{
    public required int PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public List<DateOnly> ServiceDates { get; } = new();
}
