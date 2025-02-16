using Korga.Filters.Entities;
using System;

namespace Korga.Models.Json;

public class PersonFilterRequest
{
    public required string Discriminator { get; init; }
    public int? StatusId { get; init; }
    public int? GroupId { get; init; }
    public int? GroupTypeId { get; init; }
    public int? GroupRoleId { get; init; }
    public int? PersonId { get; init; }

    public PersonFilter ToEntity()
    {
        return Discriminator switch
        {
            nameof(StatusFilter) => new StatusFilter
            {
                StatusId = StatusId.GetValueOrDefault()
            },
            nameof(GroupFilter) => new GroupFilter
            {
                GroupId = GroupId.GetValueOrDefault(),
                GroupRoleId = GroupRoleId
            },
            nameof(GroupTypeFilter) => new GroupTypeFilter
            {
                GroupTypeId = GroupTypeId.GetValueOrDefault(),
                GroupRoleId = GroupRoleId
            },
            nameof(SinglePerson) => new SinglePerson
            {
                PersonId = PersonId.GetValueOrDefault()
            },
            _ => throw new ArgumentException($"Invalid filter type {Discriminator}")
        };
    }
}
