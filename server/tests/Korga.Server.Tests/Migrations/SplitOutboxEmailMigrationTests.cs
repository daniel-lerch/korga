using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ForwardMode_DbContext = Korga.Server.Tests.Migrations.ForwardMode.DatabaseContext;
using ForwardMode_OutboxEmail = Korga.Server.Tests.Migrations.ForwardMode.OutboxEmail;
using SplitOutboxEmail_DbContext = Korga.Server.Tests.Migrations.SplitOutboxEmail.DatabaseContext;
using SplitOutboxEmail_OutboxEmail = Korga.Server.Tests.Migrations.SplitOutboxEmail.OutboxEmail;
using SplitOutboxEmail_SentEmail = Korga.Server.Tests.Migrations.SplitOutboxEmail.SentEmail;

namespace Korga.Server.Tests.Migrations;

public class SplitOutboxEmailMigrationTests : MigrationTest
{
    private const string outboxEmail1_EmailAddress = "alice@example.org";
    private readonly byte[] outboxEmail1_Content = Encoding.ASCII.GetBytes("");
    private readonly DateTime outboxEmail1_DeliveryTime = DateTime.Parse("2023-08-13 18:57:21");
    private const string outboxEmail2_EmailAddress = "bob@example.org";
    private readonly byte[] outboxEmail2_Content = Encoding.ASCII.GetBytes("");

    private readonly ForwardMode_DbContext forwardMode;
    private readonly SplitOutboxEmail_DbContext splitOutboxEmail;

    public SplitOutboxEmailMigrationTests() : base("SplitOutboxEmailMigration")
    {
        forwardMode = serviceScope.ServiceProvider.GetRequiredService<ForwardMode_DbContext>();
        splitOutboxEmail = serviceScope.ServiceProvider.GetRequiredService<SplitOutboxEmail_DbContext>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ForwardMode_DbContext>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));

        services.AddDbContext<SplitOutboxEmail_DbContext>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));
    }

    [Fact]
    public async Task TestUpgrade()
    {
        ForwardMode_OutboxEmail beforeMigrationSent = new()
        {
            InboxEmailId = null,
            EmailAddress = outboxEmail1_EmailAddress,
            Content = outboxEmail1_Content,
            DeliveryTime = outboxEmail1_DeliveryTime,
            ErrorMessage = ""
        };
        SplitOutboxEmail_SentEmail expectedSent = new()
        {
            Id = 1,
            InboxEmailId = null,
            EmailAddress = outboxEmail1_EmailAddress,
            ContentSize = outboxEmail1_Content.Length,
            DeliveryTime = outboxEmail1_DeliveryTime,
            ErrorMessage = ""
        };
        ForwardMode_OutboxEmail beforeMigrationPending = new()
        {
            InboxEmailId = null,
            EmailAddress = outboxEmail2_EmailAddress,
            Content = outboxEmail2_Content,
            DeliveryTime = default,
            ErrorMessage = null
        };
        SplitOutboxEmail_OutboxEmail expectedPending = new()
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
        forwardMode.OutboxEmails.Add(beforeMigrationSent);
        forwardMode.OutboxEmails.Add(beforeMigrationPending);
        await forwardMode.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("SplitOutboxEmail");

        // Verify that data has been migrated as expected
        SplitOutboxEmail_SentEmail sentEmail = await splitOutboxEmail.SentEmails.SingleAsync();
        SplitOutboxEmail_OutboxEmail outboxEmail = await splitOutboxEmail.OutboxEmails.SingleAsync();

        Assert.Equivalent(expectedSent, sentEmail);
        Assert.Equivalent(expectedPending, outboxEmail);
    }

    [Fact]
    public async Task TestDowngrade()
    {
        SplitOutboxEmail_SentEmail beforeDowngradeSent = new()
        {
            Id = 1,
            InboxEmailId = null,
            EmailAddress = outboxEmail1_EmailAddress,
            ContentSize = outboxEmail1_Content.Length,
            DeliveryTime = outboxEmail1_DeliveryTime,
            ErrorMessage = ""
        };
        SplitOutboxEmail_OutboxEmail beforeDowngradePending = new()
        {
            Id = 2,
            InboxEmailId = null,
            EmailAddress = outboxEmail2_EmailAddress,
            Content = outboxEmail2_Content
        };
        ForwardMode_OutboxEmail expectedPending = new()
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
        splitOutboxEmail.SentEmails.Add(beforeDowngradeSent);
        splitOutboxEmail.OutboxEmails.Add(beforeDowngradePending);
        await splitOutboxEmail.SaveChangesAsync();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("ForwardMode");

        // Verify that data has been rolled back as expected
        ForwardMode_OutboxEmail outboxEmail = await forwardMode.OutboxEmails.SingleAsync();
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
