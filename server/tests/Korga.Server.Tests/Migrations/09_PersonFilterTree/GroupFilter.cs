﻿namespace Korga.Server.Tests.Migrations.PersonFilterTree;

public class GroupFilter : PersonFilter
{
    public Group? Group { get; set; }
    public int GroupId { get; set; }

    public GroupRole? GroupRole { get; set; }
    public int? GroupRoleId { get; set; }
}
