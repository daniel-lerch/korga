using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class GroupFilter : PersonFilter
{
    public Group? Group { get; set; }
    public int GroupId { get; set; }

    public GroupRole? GroupRole { get; set; }
    public int? GroupRoleId { get; set; }

    public override bool FilterConditionEquals(PersonFilter other)
    {
        return other is GroupFilter o && GroupId == o.GroupId && GroupRoleId == o.GroupRoleId;
    }
}
