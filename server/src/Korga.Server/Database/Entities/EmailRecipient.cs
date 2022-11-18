﻿using System;

namespace Korga.Server.Database.Entities;

public class EmailRecipient
{
    public EmailRecipient(string emailAddress, string fullName)
    {
        EmailAddress = emailAddress;
        FullName = fullName;
    }

    public long Id { get; set; }

    public long EmailId { get; set; }
    public Email? Email { get; set; }

    public string EmailAddress { get; set; }
    public string FullName { get; set; }
    public DateTime DeliveryTime { get; set; }
}
