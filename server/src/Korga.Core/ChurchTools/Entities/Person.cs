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


	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
}
