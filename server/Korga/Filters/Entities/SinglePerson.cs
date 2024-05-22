using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class SinglePerson : PersonFilter
{
	public Person? Person { get; set; }
	public int PersonId { get; set; }

    public override bool FilterConditionEquals(PersonFilter other)
    {
        return other is SinglePerson o && PersonId == o.PersonId;
    }
}
