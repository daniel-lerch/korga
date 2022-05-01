using System;

namespace Korga.Server.Models.Json;

public class EventRegistrationResponse
{
    public long Id { get; set; }
    public Guid Token { get; set; }
}
