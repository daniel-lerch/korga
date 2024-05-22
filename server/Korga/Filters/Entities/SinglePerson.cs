using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class SinglePerson : PersonFilter
{
	public Person? Person { get; set; }
	public int PersonId { get; set; }

    public override PersonFilterEqualityKey GetEqualityKey()
    {
        return new PersonFilterEqualityKey(nameof(SinglePerson), PersonId: PersonId);
    }
}
