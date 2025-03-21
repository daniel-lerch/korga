﻿using System;

namespace Korga.Tests.Migrations.PersonFilterList;

public class InboxEmail
{
    public long Id { get; set; }

    public long? DistributionListId { get; set; }
    public DistributionList? DistributionList { get; set; }

    public uint? UniqueId { get; set; }
    public required string Subject { get; set; }
    public required string From { get; set; }
    public string? Sender { get; set; }
    public string? ReplyTo { get; set; }
    public required string To { get; set; }
    public string? Receiver { get; set; }
    public byte[]? Header { get; set; }
    public byte[]? Body { get; set; }
    public DateTime DownloadTime { get; set; }
    public DateTime ProcessingCompletedTime { get; set; }
}
