using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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

    public InboxOutboxMigrationTests() : base("InboxOutboxMigration") { }

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
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("AddSinglePersonFilter");

        // Add test data
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText =
@"INSERT INTO `Emails`
(`DistributionListId`, `Subject`, `From`, `Sender`, `To`, `Receiver`, `Body`, `DownloadTime`, `RecipientsFetchTime`)
VALUES (NULL, @subject, @from, NULL, @to, @receiver, @body, @downloadtime, @completedtime)
";
            command.Parameters.AddWithValue("@subject", inboxEmail_Subject);
            command.Parameters.AddWithValue("@from", inboxEmail_From);
            command.Parameters.AddWithValue("@to", inboxEmail_To);
            command.Parameters.AddWithValue("@receiver", inboxEmail_Receiver);
            command.Parameters.AddWithValue("@body", inboxEmail_Body);
            command.Parameters.AddWithValue("@downloadtime", inboxEmail_DownloadTime);
            command.Parameters.AddWithValue("@completedtime", inboxEmail_ProcessingCompletedTime);

            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        // Run migration at test
        await migrator.MigrateAsync("InboxOutbox");

        // Verify that data has been migrated as expected
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText =
@"SELECT `DistributionListId`, `UniqueId`, `Subject`, `From`, `Sender`, `ReplyTo`, `To`, `Receiver`, `Header`, `Body`, `DownloadTime`, `ProcessingCompletedTime`
FROM `InboxEmails`";
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.True(reader.IsDBNull(0));
            Assert.Equal(0u, reader.GetUInt32(1));
            Assert.Equal(inboxEmail_Subject, reader.GetString(2));
            Assert.Equal(inboxEmail_From, reader.GetString(3));
            Assert.True(reader.IsDBNull(4));
            Assert.True(reader.IsDBNull(5));
            Assert.Equal(inboxEmail_To, reader.GetString(6));
            Assert.Equal(inboxEmail_Receiver, reader.GetString(7));
            Assert.True(reader.IsDBNull(8));
            Assert.Equal(inboxEmail_Body, (byte[])reader.GetValue(9));
            Assert.Equal(inboxEmail_DownloadTime, reader.GetDateTime(10));
            Assert.Equal(inboxEmail_ProcessingCompletedTime, reader.GetDateTime(11));
        }

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
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("InboxOutbox");

        // Add test data
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText =
@"INSERT INTO `InboxEmails`
(`DistributionListId`, `UniqueId`, `Subject`, `From`, `Sender`, `ReplyTo`, `To`, `Receiver`, `Header`, `Body`, `DownloadTime`, `ProcessingCompletedTime`)
VALUES (NULL, @uniqueid, @subject, @from, NULL, NULL, @to, @receiver, @header, @body, @downloadtime, @completedtime)";
            command.Parameters.AddWithValue("@uniqueid", inboxEmail_UniqueId);
            command.Parameters.AddWithValue("@subject", inboxEmail_Subject);
            command.Parameters.AddWithValue("@from", inboxEmail_From);
            command.Parameters.AddWithValue("@to", inboxEmail_To);
            command.Parameters.AddWithValue("@receiver", inboxEmail_Receiver);
            command.Parameters.AddWithValue("@header", inboxEmail_Header);
            command.Parameters.AddWithValue("@body", inboxEmail_Body);
            command.Parameters.AddWithValue("@downloadtime", inboxEmail_DownloadTime);
            command.Parameters.AddWithValue("@completedtime", inboxEmail_ProcessingCompletedTime);

            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("AddSinglePersonFilter");

        // Verify that data has been rolled back as expected
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText =
@"SELECT `DistributionListId`, `Subject`, `From`, `Sender`, `To`, `Receiver`, `Body`, `DownloadTime`, `RecipientsFetchTime`
FROM `Emails`";
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.True(reader.IsDBNull(0));
            Assert.Equal(inboxEmail_Subject, reader.GetString(1));
            Assert.Equal(inboxEmail_From, reader.GetString(2));
            Assert.True(reader.IsDBNull(3));
            Assert.Equal(inboxEmail_To, reader.GetString(4));
            Assert.Equal(inboxEmail_Receiver, reader.GetString(5));
            Assert.Equal(inboxEmail_Body, (byte[])reader.GetValue(6));
            Assert.Equal(inboxEmail_DownloadTime, reader.GetDateTime(7));
            Assert.Equal(inboxEmail_ProcessingCompletedTime, reader.GetDateTime(8));
        }

        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SHOW TABLES WHERE `Tables_in_{databaseName}` = \"InboxEmails\"";
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            Assert.False(await reader.ReadAsync());
        }
    }
}
