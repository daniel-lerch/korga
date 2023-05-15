using System.Text.Json.Serialization;

namespace Korga.Server.Models.Json;

public class LoginRequest
{
    [JsonConstructor]
    public LoginRequest(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; set; }
    public string Password { get; set; }
}
