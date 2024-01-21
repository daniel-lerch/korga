namespace Korga.ChurchTools.Entities;

public class GroupMember : IIdentifiable<long>
{
    public int PersonId { get; set; }
	public Person? Person { get; set; }

	public int GroupId { get; set; }
	public Group? Group { get; set; }

	public int GroupRoleId { get; set; }
	public GroupRole? GroupRole { get; set; }

    public GroupMemberStatus GroupMemberStatus { get; set; }

	long IIdentifiable<long>.Id => (((long)PersonId) << 32) | (long)GroupId;
}
