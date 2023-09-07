using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Korga.ChurchTools.Api;

public class PersonMasterdata
{
    public PersonMasterdata()
    {
        Roles = Array.Empty<Role>();
        GroupTypes = Array.Empty<GroupType>();
        GroupStatuses = Array.Empty<GroupStatus>();
        Departments = Array.Empty<Department>();
        Statuses = Array.Empty<Status>();
    }

    [JsonConstructor]
    public PersonMasterdata(IReadOnlyList<Role> roles, IReadOnlyList<GroupType> groupTypes, IReadOnlyList<GroupStatus> groupStatuses, IReadOnlyList<Department> departments, IReadOnlyList<Status> statuses)
    {
        Roles = roles;
        GroupTypes = groupTypes;
        GroupStatuses = groupStatuses;
        Departments = departments;
        Statuses = statuses;
    }

    public IReadOnlyList<Role> Roles { get; set; }
	public IReadOnlyList<GroupType> GroupTypes { get; set; }
    public IReadOnlyList<GroupStatus> GroupStatuses { get; set; }
	public IReadOnlyList<Department> Departments { get; set; }
	public IReadOnlyList<Status> Statuses { get; set; }

	public record Role(int Id, int GroupTypeId, string Name, int SortKey) : IIdentifiable<int>;
	public record GroupType(int Id, string Name, int SortKey) : IIdentifiable<int>;
    public record GroupStatus(int Id, string Name, int SortKey) : IIdentifiable<int>;
	public record Department(int Id, string Name, int SortKey) : IIdentifiable<int>;
	public record Status(int Id, string Name, int SortKey) : IIdentifiable<int>;
}
