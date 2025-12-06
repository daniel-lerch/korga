namespace ChurchTools.Model;

public class GroupsMember
{
    public GroupsMember(int personId, int groupId, int groupTypeRoleId, string groupMemberStatus)
    {
        PersonId = personId;
        GroupId = groupId;
        GroupTypeRoleId = groupTypeRoleId;
        GroupMemberStatus = groupMemberStatus;
    }

    public int PersonId { get; set; }
    public int GroupId { get; set; }
    public int GroupTypeRoleId { get; set; }
    public string GroupMemberStatus { get; set; }
}
