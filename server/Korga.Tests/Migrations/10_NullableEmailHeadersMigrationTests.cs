using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Tests.Migrations;

public class NullableEmailHeadersMigrationTests : MigrationTestBase<PersonFilterList.DatabaseContext, NullableEmailHeaders.DatabaseContext>
{
    public NullableEmailHeadersMigrationTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact]
    public async Task TestUpgrade()
    {
        PersonFilterList.InboxEmail reference = new()
        {
            Id = 1,
            Subject = "Test",
            From = "alice@example.org",
            To = "bob@example.org",
        };
        PersonFilterList.InboxEmail emptyFields = new()
        {
            Id = 2,
            Subject = string.Empty,
            From = string.Empty,
            To = string.Empty,
        };
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("PersonFilterList");

        // Add test data
        before.InboxEmails.Add(reference);
        before.InboxEmails.Add(emptyFields);
        await before.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("NullableEmailHeaders");

        // Verify that data has been migrated as expected
        List<NullableEmailHeaders.InboxEmail> inboxEmails = await after.InboxEmails.ToListAsync();
        Assert.Contains(inboxEmails, email => email.Id == reference.Id);
        Assert.Contains(inboxEmails, email => email.Id == emptyFields.Id);
        Assert.Equal(2, inboxEmails.Count);
    }

    [Fact]
    public async Task TestDowngrade()
    {
        NullableEmailHeaders.InboxEmail reference = new()
        {
            Id = 1,
            Subject = "Test",
            From = "alice@example.org",
            To = "bob@example.org",
        };
        NullableEmailHeaders.InboxEmail emptyFields = new()
        {
            Id = 2,
            Subject = string.Empty,
            From = string.Empty,
            To = string.Empty,
        };
        NullableEmailHeaders.InboxEmail nullSubject = new()
        {
            Id = 3,
            Subject = null,
            From = "alice@example.org",
            To = "bob@example.org",
        };
        NullableEmailHeaders.InboxEmail nullFrom = new()
        {
            Id = 4,
            Subject = "Test",
            From = null,
            To = "bob@example.org",
        };
        NullableEmailHeaders.InboxEmail nullTo = new()
        {
            Id = 5,
            Subject = "Test",
            From = "alice@example.org",
            To = null,
        };
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("NullableEmailHeaders");

        // Add test data
        after.InboxEmails.Add(reference);
        after.InboxEmails.Add(emptyFields);
        after.InboxEmails.Add(nullSubject);
        after.InboxEmails.Add(nullFrom);
        after.InboxEmails.Add(nullTo);
        await after.SaveChangesAsync();

        // Reset change tracker before upgrading the schema again to avoid caching
        after.ChangeTracker.Clear();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("PersonFilterList");

        // Verify that data has been migrated as expected
        List<PersonFilterList.InboxEmail> inboxEmails = await before.InboxEmails.ToListAsync();
        Assert.Contains(inboxEmails, email => email.Id == reference.Id);
        Assert.Contains(inboxEmails, email => email.Id == emptyFields.Id);
        Assert.DoesNotContain(inboxEmails, email => email.Id == nullSubject.Id);
        Assert.DoesNotContain(inboxEmails, email => email.Id == nullFrom.Id);
        Assert.DoesNotContain(inboxEmails, email => email.Id == nullTo.Id);
        Assert.Equal(2, inboxEmails.Count);

        // Upgrade database again to verify rollback worked
        await migrator.MigrateAsync("NullableEmailHeaders");
    }
}
