﻿using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailDelivery;

public class EmailDeliveryService
{
    private readonly DatabaseContext database;
    private readonly JobQueue<EmailDeliveryJobController> jobQueue;

    public EmailDeliveryService(DatabaseContext database, JobQueue<EmailDeliveryJobController> jobQueue)
    {
        this.database = database;
        this.jobQueue = jobQueue;
    }

    public async ValueTask<bool> Enqueue(string emailAddress, MimeMessage mimeMessage, long? inboxEmailId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("The recipient address must not be null or empty", nameof(emailAddress));

        if (inboxEmailId.HasValue && await database.OutboxEmails
                .AnyAsync(email => email.EmailAddress == emailAddress && email.InboxEmailId == inboxEmailId, cancellationToken))
            return false;

        byte[] content;

        using (MemoryStream memoryStream = new())
        {
            mimeMessage.WriteTo(memoryStream, CancellationToken.None);
            content = memoryStream.ToArray();
        }

        database.OutboxEmails.Add(new(emailAddress, content) { InboxEmailId = inboxEmailId});
        await database.SaveChangesAsync(cancellationToken);
        jobQueue.EnsureRunning();
        return true;
    }
}
