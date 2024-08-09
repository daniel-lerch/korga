using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Korga.Tests.Migrations;

public class PermissionMigrationTests : MigrationTestBase<PersonFilterList.DatabaseContext, Permissions.DatabaseContext>
{
    public PermissionMigrationTests(ITestOutputHelper testOutput) : base(testOutput) { }

    [Fact]
    public async Task TestUpgrade()
    {
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("PersonFilterList");

        // Add test data
        PersonFilterList.DistributionList distributionList = new()
        {
            Alias = "test",
            PermittedRecipients = new(),
        };
        before.DistributionLists.Add(distributionList);
        await before.SaveChangesAsync();

        long permittedRecipientId = distributionList.PermittedRecipients.Id;

        // Reset change tracker before upgrading the schema again to avoid caching
        before.ChangeTracker.Clear();

        // Run migration at test
        await migrator.MigrateAsync("Permissions");

        List<Permissions.Permission> permissions =
            await after.Permissions.Include(p => p.PersonFilterList).ToListAsync();

        // Don't use Enum.GetValues here as may change in the future.
        Assert.Contains(permissions, p => p.Key == Filters.Permissions.Permissions_View);
        Assert.Contains(permissions, p => p.Key == Filters.Permissions.Permissions_Admin);
        Assert.Contains(permissions, p => p.Key == Filters.Permissions.DistributionLists_View);
        Assert.Contains(permissions, p => p.Key == Filters.Permissions.DistributionLists_Admin);
        Assert.Contains(permissions, p => p.Key == Filters.Permissions.ServiceHistory_View);

        Assert.True(await after.PersonFilterLists.AnyAsync(fl => fl.Id == permittedRecipientId));
    }

    [Fact]
    public async Task TestDowngrade()
    {
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("Permissions");

        // Add test data
        Permissions.DistributionList distributionList = new()
        {
            Alias = "test",
            PermittedRecipients = new(),
            PermittedSenders = new(),
        };
        after.DistributionLists.Add(distributionList);
        await after.SaveChangesAsync();

        long permittedRecipientId = distributionList.PermittedRecipients.Id;
        long permittedSenderId = distributionList.PermittedSenders.Id;
        List<long> permissionFilters = await after.Permissions.Select(p => p.PersonFilterListId).ToListAsync();

        // Reset change tracker before upgrading the schema again to avoid caching
        after.ChangeTracker.Clear();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("PersonFilterList");

        List<long> filters = await before.PersonFilterLists.Select(fl => fl.Id).ToListAsync();

        foreach (long filter in permissionFilters)
        {
            Assert.DoesNotContain(filter, filters);
        }

        Assert.Contains(permittedRecipientId, filters);
        Assert.DoesNotContain(permittedSenderId, filters);
    }
}
