namespace Korga.Filters.Entities;

public class Permission
{
    public Permission(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    // If PersonFilterList is null or empty, users might only transitively have this permission
    public long? PersonFilterListId { get; set; }
    public PersonFilterList? PersonFilterList { get; set; }
}
