﻿namespace Korga.Server.Tests.Migrations.PersonFilterTree;

public class DistributionList
{
    public required long Id { get; set; }
    public required string Alias { get; set; }
    public int Flags { get; set; }
    public long? PermittedRecipientsId { get; set; }
}
