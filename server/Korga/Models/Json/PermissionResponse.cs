using Korga.Filters;
using System.Collections.Generic;

namespace Korga.Models.Json;

public class PermissionResponse
{
    public required Permissions Key { get; init; }
    public required IReadOnlyList<PersonFilterResponse> PersonFilters { get; init; }
}
