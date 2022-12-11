using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Korga.Server.ChurchTools;

public class Group
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public Dictionary<string, JsonElement> Information { get; set; }
}
