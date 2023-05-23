using System;
using System.Collections.Generic;

namespace Korga.ChurchTools.Entities;

public class Person : IIdentifiable<int>
{
	public Person(int id, int statusId, string firstName, string lastName, string email)
	{
		Id = id;
		StatusId = statusId;
		FirstName = firstName;
		LastName = lastName;
		Email = email;
	}

	public int Id { get; set; }

	public int StatusId { get; set; }
	public Status? Status { get; set; }

	public IEnumerable<DepartmentMember>? Departments { get; set; }

	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }

	public override bool Equals(object? obj)
	{
		return obj is Person person &&
			   Id == person.Id &&
			   StatusId == person.StatusId &&
			   FirstName == person.FirstName &&
			   LastName == person.LastName &&
			   Email == person.Email;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, StatusId, FirstName, LastName, Email);
	}
}
