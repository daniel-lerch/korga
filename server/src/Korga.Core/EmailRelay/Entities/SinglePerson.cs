using Korga.ChurchTools.Entities;

namespace Korga.EmailRelay.Entities;

public class SinglePerson : PersonFilter
{
	public Person? Person { get; set; }
	public int PersonId { get; set; }
}
