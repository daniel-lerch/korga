using System.Text.Json.Serialization;

namespace Korga.Models.Json;

public class PasswordResetInfo
{
    [JsonConstructor]
    public PasswordResetInfo(string uid, string givenName, string familyName)
    {
        Uid = uid;
        GivenName = givenName;
        FamilyName = familyName;
    }

    public string Uid { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
}
