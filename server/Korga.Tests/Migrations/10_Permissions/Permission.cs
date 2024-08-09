namespace Korga.Tests.Migrations.Permissions;

public class Permission
{
    public Filters.Permissions Key { get; set; }
    public long PersonFilterListId { get; set; }
    public PersonFilterList? PersonFilterList { get; set; }
}
