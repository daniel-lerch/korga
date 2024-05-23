using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Tests.Migrations;

public class SplitOutboxEmailMigrationTests : MigrationTestBase<ForwardMode.DatabaseContext, SplitOutboxEmail.DatabaseContext>
{
    private const string outboxEmail1_EmailAddress = "alice@example.org";
    private readonly byte[] outboxEmail1_Content = Encoding.ASCII.GetBytes("");
    private readonly DateTime outboxEmail1_DeliveryTime = DateTime.Parse("2023-08-13 18:57:21");
    private const string outboxEmail2_EmailAddress = "bob@example.org";
    private readonly byte[] outboxEmail2_Content = Encoding.ASCII.GetBytes("");

    [Fact]
    public async Task TestUpgrade()
    {
        ForwardMode.OutboxEmail beforeMigrationSent = new()
        {
            InboxEmailId = null,
            EmailAddress = outboxEmail1_EmailAddress,
            Content = outboxEmail1_Content,
            DeliveryTime = outboxEmail1_DeliveryTime,
            ErrorMessage = ""
        };
        SplitOutboxEmail.SentEmail expectedSent = new()
        {
            Id = 1,
            InboxEmailId = null,
            EmailAddress = outboxEmail1_EmailAddress,
            ContentSize = outboxEmail1_Content.Length,
            DeliveryTime = outboxEmail1_DeliveryTime,
            ErrorMessage = ""
        };
        ForwardMode.OutboxEmail beforeMigrationPending = new()
        {
            InboxEmailId = null,
            EmailAddress = outboxEmail2_EmailAddress,
            Content = outboxEmail2_Content,
            DeliveryTime = default,
            ErrorMessage = null
        };
        SplitOutboxEmail.OutboxEmail expectedPending = new()
        {
            Id = 2,
            InboxEmailId = null,
            EmailAddress = outboxEmail2_EmailAddress,
            Content = outboxEmail2_Content
        };

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("ForwardMode");

        // Add test data
        before.OutboxEmails.Add(beforeMigrationSent);
        before.OutboxEmails.Add(beforeMigrationPending);
        await before.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("SplitOutboxEmail");

        // Verify that data has been migrated as expected
        SplitOutboxEmail.SentEmail sentEmail = await after.SentEmails.SingleAsync();
        SplitOutboxEmail.OutboxEmail outboxEmail = await after.OutboxEmails.SingleAsync();

        Assert.Equivalent(expectedSent, sentEmail);
        Assert.Equivalent(expectedPending, outboxEmail);
    }

    [Fact]
    public async Task TestDowngrade()
    {
        SplitOutboxEmail.SentEmail beforeDowngradeSent = new()
        {
            Id = 1,
            InboxEmailId = null,
            EmailAddress = outboxEmail1_EmailAddress,
            ContentSize = outboxEmail1_Content.Length,
            DeliveryTime = outboxEmail1_DeliveryTime,
            ErrorMessage = ""
        };
        SplitOutboxEmail.OutboxEmail beforeDowngradePending = new()
        {
            Id = 2,
            InboxEmailId = null,
            EmailAddress = outboxEmail2_EmailAddress,
            Content = outboxEmail2_Content
        };
        ForwardMode.OutboxEmail expectedPending = new()
        {
            Id = 2,
            InboxEmailId = null,
            EmailAddress = outboxEmail2_EmailAddress,
            Content = outboxEmail2_Content,
            DeliveryTime = default,
            ErrorMessage = null
        };

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("SplitOutboxEmail");

        // Add test data
        after.SentEmails.Add(beforeDowngradeSent);
        after.OutboxEmails.Add(beforeDowngradePending);
        await after.SaveChangesAsync();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("ForwardMode");

        // Verify that data has been rolled back as expected
        ForwardMode.OutboxEmail outboxEmail = await before.OutboxEmails.SingleAsync();
        Assert.Equivalent(expectedPending, outboxEmail);

        // Make sure old table has been deleted
        await using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"SHOW TABLES WHERE `Tables_in_{databaseName}` = \"SentEmails\"";
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            Assert.False(await reader.ReadAsync());
        }
    }
}
