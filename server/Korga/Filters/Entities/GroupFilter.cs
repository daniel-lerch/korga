using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class GroupFilter : PersonFilter
{
    public Group? Group { get; set; }
    public int GroupId { get; set; }

    public GroupRole? GroupRole { get; set; }
    public int? GroupRoleId { get; set; }

    public override PersonFilterEqualityKey GetEqualityKey()
    {
        return new PersonFilterEqualityKey(nameof(GroupFilter), GroupId: GroupId, GroupRoleId: GroupRoleId);
    }
}
