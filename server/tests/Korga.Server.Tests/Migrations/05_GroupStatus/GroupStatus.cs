﻿using System;

namespace Korga.Server.Tests.Migrations.GroupStatus;

public class GroupStatus
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public DateTime DeletionTime { get; set; }
}
