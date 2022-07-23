using System;

namespace Korga.Server.EmailRelay.Entities;

public class Email
{
    public long Id { get; set; }
    public DateTime DownloadTime { get; set; }
    public DateTime RecipientsFetchTime { get; set; }
    public DateTime DeliveryTime { get; set; }
}
