using System;
using System.Collections.Generic;
using Korga.EmailDelivery.Entities;

namespace Korga.EmailRelay.Entities;

public class InboxEmail
{
    public InboxEmail(uint uniqueId, string subject, string from, string? sender, string? replyTo, string to, string? receiver, byte[]? header, byte[]? body)
    {
        UniqueId = uniqueId;
        Subject = subject;
        From = from;
        Sender = sender;
        ReplyTo = replyTo;
        To = to;
        Receiver = receiver;
        Header = header;
        Body = body;
    }

    public long Id { get; set; }

    public long? DistributionListId { get; set; }
    public DistributionList? DistributionList { get; set; }

    public uint UniqueId { get; set; }
    public string Subject { get; set; }
    public string From { get; set; }
    public string? Sender { get; set; }
    public string? ReplyTo { get; set; }
    public string To { get; set; }
    public string? Receiver { get; set; }
    public byte[]? Header { get; set; }
    public byte[]? Body { get; set; }
    public DateTime DownloadTime { get; set; }
    public DateTime ProcessingCompletedTime { get; set; }

    public IEnumerable<OutboxEmail>? Recipients { get; set; }
}
