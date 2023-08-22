using System.Collections.Generic;

namespace Korga.ChurchTools.Api;

public class PersonMasterdata
{
    public PersonMasterdata(List<Role> roles, List<GroupType> groupTypes, List<GroupStatus> groupStatuses, List<Department> departments, List<Status> statuses)
    {
        Roles = roles;
        GroupTypes = groupTypes;
        GroupStatuses = groupStatuses;
        Departments = departments;
        Statuses = statuses;
    }

    public List<Role> Roles { get; set; }
	public List<GroupType> GroupTypes { get; set; }
    public List<GroupStatus> GroupStatuses { get; set; }
	public List<Department> Departments { get; set; }
	public List<Status> Statuses { get; set; }

	public record Role(int Id, int GroupTypeId, string Name, int SortKey) : IIdentifiable<int>;
	public record GroupType(int Id, string Name, int SortKey) : IIdentifiable<int>;
    public record GroupStatus(int Id, string Name, int SortKey) : IIdentifiable<int>;
	public record Department(int Id, string Name, int SortKey) : IIdentifiable<int>;
	public record Status(int Id, string Name, int SortKey) : IIdentifiable<int>;
}
