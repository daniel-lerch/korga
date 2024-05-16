namespace ChurchTools.Model;

public class GroupMember : IIdentifiable<long>
{
    public GroupMember(int personId, int groupId, int groupTypeRoleId, string groupMemberStatus)
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

    long IIdentifiable<long>.Id => (long)PersonId << 32 | (long)GroupId;
}
