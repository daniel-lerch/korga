namespace Korga.Server.ChurchTools.Api;

public class Person
{
	public Person(int id, string firstName, string lastName, string email)
	{
		Id = id;
		FirstName = firstName;
		LastName = lastName;
		Email = email;
	}

	public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
