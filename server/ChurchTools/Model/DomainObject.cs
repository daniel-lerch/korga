using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace ChurchTools.Model;

public record DomainObject(
    string Title,
    string DomainType,
    string DomainIdentifier,
    Dictionary<string, JsonValue> DomainAttributes);
