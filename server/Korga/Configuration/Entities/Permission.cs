using Korga.Filters.Entities;

namespace Korga.Configuration.Entities;

public class Permission
{
    public Permission(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public long PersonFilterListId { get; set; }
    public PersonFilterList? PersonFilterList { get; set; }
}
