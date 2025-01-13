using System.Text.Json.Serialization;

namespace ChurchTools.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GroupMemberStatus
{
    Active,
    Requested,
    Waiting,
    To_Delete // Custom JSON values are not supported by System.Text.Json: https://stackoverflow.com/a/59061296
}
