namespace Korga.Tests.Migrations.PersonFilterList;

public abstract class PersonFilter
{
    public long Id { get; set; }

    public long PersonFilterListId { get; set; }
    public PersonFilterList? PersonFilterList { get; set; }
}
