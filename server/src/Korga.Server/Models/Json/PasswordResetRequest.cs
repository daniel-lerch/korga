using System;
using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json;

public class PasswordResetRequest
{
    [JsonConstructor]
    public PasswordResetRequest(Guid token, string passwordHash)
    {
        Token = token;
        PasswordHash = passwordHash;
    }

    public Guid Token { get; set; }
    public string PasswordHash { get; set; }
}
