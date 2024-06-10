﻿using Korga.EmailDelivery.Entities;
using Korga.Utilities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.EmailDelivery;

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

        OutboxEmail outboxEmail = new(emailAddress, content) { InboxEmailId = inboxEmailId };
        database.OutboxEmails.Add(outboxEmail);
        await database.SaveChangesAsync(cancellationToken);

        // Explicitly free entity for garbage collection because our DbContext won't be disposed soon enough
        // Without this line, Korga takes gigabytes of memory when sending large messages to many recipients
        database.Entry(outboxEmail).State = EntityState.Detached;

        jobQueue.EnsureRunning();
        return true;
    }
}
