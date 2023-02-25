using System;
using System.Collections.Generic;

namespace Korga.EmailRelay.Entities;

public class Email
{
    public Email(string subject, string from, string? sender, string to, string? receiver, byte[] body)
    {
        Subject = subject;
        From = from;
        Sender = sender;
        To = to;
        Receiver = receiver;
        Body = body;
    }

    public long Id { get; set; }

    public long? DistributionListId { get; set; }
    public DistributionList? DistributionList { get; set; }

    public string Subject { get; set; }
    public string From { get; set; }
    public string? Sender { get; set; }
    public string To { get; set; }
    public string? Receiver { get; set; }
    public byte[] Body { get; set; }
    public DateTime DownloadTime { get; set; }
    public DateTime RecipientsFetchTime { get; set; }

    public IEnumerable<EmailRecipient>? Recipients { get; set; }
}
