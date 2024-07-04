using ChurchTools.Model;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.Models.Json;

public class ServiceHistoryResponse
{
    public required int PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required List<GroupInfo> Groups { get; init; }

    public List<ServiceDate> ServiceDates { get; } = [];

    public readonly struct GroupInfo
    {
        public required int GroupId { get; init; }
        public required string GroupName { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required GroupMemberStatus GroupMemberStatus { get; init; }
        public required string Comment { get; init; }
    }

    public readonly struct ServiceDate
    {
        public required int ServiceId { get; init; }
        public required DateOnly Date { get; init; }
    }
}
