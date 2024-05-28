using System.Collections.Generic;

namespace Korga.Filters.Entities;

public class PersonFilterList
{
    public long Id { get; set; }

    public List<PersonFilter>? Filters { get; set; }
}
