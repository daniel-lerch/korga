using System.Collections.Generic;

namespace Korga.ChurchTools.Api;

public class Person : IIdentifiable<int>
{
	public Person(int id, int statusId, List<int> departmentIds, string firstName, string lastName, string email)
	{
		Id = id;
		StatusId = statusId;
		DepartmentIds = departmentIds;
		FirstName = firstName;
		LastName = lastName;
		Email = email;
	}

	public int Id { get; set; }
	public int StatusId { get; set; }
	public List<int> DepartmentIds { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
