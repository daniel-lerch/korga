using Mailist.ChurchTools.Entities;

namespace Mailist.Filters.Entities;

public class SinglePerson : PersonFilter
{
	public Person? Person { get; set; }
	public int PersonId { get; set; }
}
