using System.Collections.Generic;

namespace Mailist.Filters.Entities;

public class PersonFilterList
{
    public long Id { get; set; }

    public List<PersonFilter>? Filters { get; set; }
}
