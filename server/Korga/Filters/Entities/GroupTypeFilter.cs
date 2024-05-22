using Korga.ChurchTools.Entities;

namespace Korga.Filters.Entities;

public class GroupTypeFilter : PersonFilter
{
    public GroupType? GroupType { get; set; }
    public int? GroupTypeId { get; set; }

    public GroupRole? GroupRole { get; set; }
    public int? GroupRoleId { get; set; }

    public override bool FilterConditionEquals(PersonFilter other)
    {
        return other is GroupTypeFilter o && GroupTypeId == o.GroupTypeId && GroupRoleId == o.GroupRoleId;
    }
}
