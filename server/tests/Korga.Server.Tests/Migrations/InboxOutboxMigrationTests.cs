﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using AddSinglePersonFilter_DbContext = Korga.Server.Tests.Migrations.AddSinglePersonFilter.DatabaseContext;
using AddSinglePersonFilter_Email = Korga.Server.Tests.Migrations.AddSinglePersonFilter.Email;
using InboxOutbox_DbContext = Korga.Server.Tests.Migrations.InboxOutbox.DatabaseContext;
using InboxOutbox_InboxEmail = Korga.Server.Tests.Migrations.InboxOutbox.InboxEmail;

namespace Korga.Server.Tests.Migrations;

/// <summary>
/// These tests focus on breaking database schema changes
/// that would result in data loss if not handled manually.
/// 
/// Non-breaking changes like creating columns are handled by generated code
/// which is covered by Microsoft's unit tests.
/// </summary>
public class InboxOutboxMigrationTests : MigrationTest
{
    private const uint inboxEmail_UniqueId = 573;
    private const string inboxEmail_Subject = "Korga is amazing!";
    private const string inboxEmail_From = "Alice <alice@example.org>";
    private const string inboxEmail_To = "<admin@example.org>";
    private const string inboxEmail_Receiver = "admin@example.org";
    private static readonly byte[] inboxEmail_Header = Encoding.ASCII.GetBytes(
@"X-Envelope-From: <alice@example.org>
X-Envelope-To: <admin@example.org>
X-Delivery-Time: 1691127648
X-UID: 573
Return-Path: <alice@example.org>
Received: from mo4-p07-ob.smtp.rzone.de ([85.215.255.115])
    by smtpin.rzone.de (RZmta 49.6.6 OK)
    with ESMTPS id x94474z745emBAI
    (using TLSv1.3 with cipher TLS_AES_256_GCM_SHA384 (256 bits))
    (Client CN ""*.smtp.rzone.de"", Issuer ""Telekom Security ServerID OV Class 2 CA"" (verified OK (+EmiG)))
        (Client hostname verified OK)
    for <admin@example.org>;
    Fri, 4 Aug 2023 07:40:48 +0200 (CEST)
Received: from Daniel
    by smtp.strato.de (RZmta 49.6.6 AUTH)
    with ESMTPSA id jbaa7fz745elswK
        (using TLSv1.2 with cipher ECDHE-RSA-AES256-GCM-SHA384 (256 bits))
        (Client did not present a certificate)
    for <admin@example.org>;
    Fri, 4 Aug 2023 07:40:47 +0200 (CEST)
From: Alice <alice@example.org>
To: <admin@example.org>
Subject: Korga is amazing!
Date: Thu, 3 Aug 2023 22:40:54 -0700
Message-ID: <000601d9c696$3d776850$b86638f0$@example.org>
MIME-Version: 1.0
Content-Type: text/plain;
        charset=""us-ascii""
Content-Transfer-Encoding: 7bit
X-Mailer: Microsoft Outlook 16.0
Thread-Index: AdnGlciACcvt+J5wRaKLB5djqUAoHw==
Content-Language: de");
    private static readonly byte[] inboxEmail_Body = Encoding.ASCII.GetBytes(
@"Content-Type: text/plain;
        charset=""us-ascii""
Content-Transfer-Encoding: 7bit
Content-Language: en

Hey,
are you using ChurchTools in your church and have been wondering whether
email distribution lists can be done easier? Korga is what you need. It
imports person data and groups from ChurchTools and offers distribution
lists that you can send emails to from your favorite client like
youth@example.org.
Cheers,
Daniel");
    private static readonly DateTime inboxEmail_DownloadTime = DateTime.Parse("2023-08-03 22:42:00.797007");
    private static readonly DateTime inboxEmail_ProcessingCompletedTime = DateTime.Parse("2023-08-04 05:42:00.881735");

    private readonly AddSinglePersonFilter_DbContext addSinglePersonFilter;
    private readonly InboxOutbox_DbContext inboxOutbox;

    public InboxOutboxMigrationTests() : base("InboxOutboxMigration")
    {
        addSinglePersonFilter = serviceScope.ServiceProvider.GetRequiredService<AddSinglePersonFilter_DbContext>();
        inboxOutbox = serviceScope.ServiceProvider.GetRequiredService<InboxOutbox_DbContext>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AddSinglePersonFilter_DbContext>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));

        services.AddDbContext<InboxOutbox_DbContext>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));
    }

    /// <summary>
    /// Tests custom migration code from Emails table to InboxEmails.
    /// </summary>
    /// <remarks>
    /// The migration InboxOutbox renamed the Emails table to InboxEmails
    /// and replaced the EmailRecipients table with OutboxEmails.
    /// 
    /// Received emails from the Emails table must be preversed
    /// whereas sent and unsent emails from EmailRecipients table are discarded.
    /// </remarks> 
    [Fact]
    public async Task TestUpgrade()
    {
        AddSinglePersonFilter_Email beforeUpgrade = new()
        {
            DistributionListId = null,
            Subject = inboxEmail_Subject,
            From = inboxEmail_From,
            Sender = null,
            To = inboxEmail_To,
            Receiver = inboxEmail_Receiver,
            Body = inboxEmail_Body,
            DownloadTime = inboxEmail_DownloadTime,
            RecipientsFetchTime = inboxEmail_ProcessingCompletedTime
        };
        InboxOutbox_InboxEmail expected = new()
        {
            Id = 1,
            DistributionListId = null,
            UniqueId = 0u,
            Subject = inboxEmail_Subject,
            From = inboxEmail_From,
            Sender = null,
            ReplyTo = null,
            To = inboxEmail_To,
            Receiver = inboxEmail_Receiver,
            Header = null,
            Body = inboxEmail_Body,
            DownloadTime = inboxEmail_DownloadTime,
            ProcessingCompletedTime = inboxEmail_ProcessingCompletedTime
        };

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("AddSinglePersonFilter");

        // Add test data
        addSinglePersonFilter.Emails.Add(beforeUpgrade);
        await addSinglePersonFilter.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("InboxOutbox");

        // Verify that data has been migrated as expected
        InboxOutbox_InboxEmail inboxEmail = await inboxOutbox.InboxEmails.SingleAsync();
        Assert.Equivalent(expected, inboxEmail);

        // Make sure old table has been deleted
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SHOW TABLES WHERE `Tables_in_{databaseName}` = \"Emails\"";
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            Assert.False(await reader.ReadAsync());
        }
    }

    /// <summary>
    /// Tests custom rollback code from InboxEmails table to Emails.
    /// </summary>
    /// <remarks>
    /// The migration InboxOutbox renamed the Emails table to InboxEmails
    /// and replaced the EmailRecipients table with OutboxEmails.
    /// 
    /// Received emails from the Emails table must be preversed
    /// whereas sent and unsent emails from EmailRecipients table are discarded.
    /// </remarks> 
    [Fact]
    public async Task TestDowngrade()
    {
        InboxOutbox_InboxEmail beforeDowngrade = new()
        {
            DistributionListId = null,
            UniqueId = inboxEmail_UniqueId,
            Subject = inboxEmail_Subject,
            From = inboxEmail_From,
            Sender = null,
            ReplyTo = null,
            To = inboxEmail_To,
            Receiver = inboxEmail_Receiver,
            Header = inboxEmail_Header,
            Body = inboxEmail_Body,
            DownloadTime = inboxEmail_DownloadTime,
            ProcessingCompletedTime = inboxEmail_ProcessingCompletedTime,
        };
        AddSinglePersonFilter_Email expected = new()
        {
            Id = 1,
            DistributionListId = null,
            Subject = inboxEmail_Subject,
            From = inboxEmail_From,
            Sender = null,
            To = inboxEmail_To,
            Receiver = inboxEmail_Receiver,
            Body = inboxEmail_Body,
            DownloadTime = inboxEmail_DownloadTime,
            RecipientsFetchTime = inboxEmail_ProcessingCompletedTime
        };

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("InboxOutbox");

        // Add test data
        inboxOutbox.InboxEmails.Add(beforeDowngrade);
        await inboxOutbox.SaveChangesAsync();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("AddSinglePersonFilter");

        // Verify that data has been rolled back as expected
        AddSinglePersonFilter_Email email = await addSinglePersonFilter.Emails.SingleAsync();
        Assert.Equivalent(expected, email);

        // Make sure old table has been deleted
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SHOW TABLES WHERE `Tables_in_{databaseName}` = \"InboxEmails\"";
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            Assert.False(await reader.ReadAsync());
        }
    }
}