using System.Collections.Generic;

namespace Korga.Tests.Migrations.PersonFilterList;

public class PersonFilterList
{
    public long Id { get; set; }

    public List<PersonFilter>? Filters { get; set; }
}
