namespace Korga.ChurchTools.Api;

public class GroupMember : IIdentifiable<long>
{
	public GroupMember(int personId, int groupId, int groupTypeRoleId)
	{
		PersonId = personId;
		GroupId = groupId;
		GroupTypeRoleId = groupTypeRoleId;
	}

	public int PersonId { get; set; }
    public int GroupId { get; set; }
    public int GroupTypeRoleId { get; set; }

	long IIdentifiable<long>.Id => (((long)PersonId) << 32) | (long)GroupId;
}
