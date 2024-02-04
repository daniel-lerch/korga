using System.Collections.Generic;

namespace Korga.Server.Tests.Migrations.PersonFilterTree;

public abstract class PersonFilter
{
    public long Id { get; set; }

    public long? ParentId { get; set; }
    public PersonFilter? Parent { get; set; }

    public IEnumerable<PersonFilter>? Children { get; set; }
}
