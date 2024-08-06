namespace Korga.Filters.Entities;

public class Permission
{
    public Permission(Permissions key)
    {
        Key = key;
    }

    public Permissions Key { get; set; }

    // If PersonFilterList is empty, users might only transitively have this permission
    public long PersonFilterListId { get; set; }
    public PersonFilterList? PersonFilterList { get; set; }
}
