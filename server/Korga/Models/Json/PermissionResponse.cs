using System.Collections.Generic;

namespace Korga.Models.Json;

public class PermissionResponse
{
    public required string Key { get; init; }
    public required IReadOnlyList<PersonFilterResponse> PersonFilters { get; init; }
}
