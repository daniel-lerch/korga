using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
namespace Korga.Server.Tests.Migrations;

public class GroupStatusMigrationTests : MigrationTest
{
    private readonly SplitOutboxEmail.DatabaseContext splitOutboxEmail;
    private readonly GroupStatus.DatabaseContext groupStatus;

    public GroupStatusMigrationTests() : base("GroupStatusMigration")
    {
        splitOutboxEmail = serviceScope.ServiceProvider.GetRequiredService<SplitOutboxEmail.DatabaseContext>();
        groupStatus = serviceScope.ServiceProvider.GetRequiredService<GroupStatus.DatabaseContext>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<SplitOutboxEmail.DatabaseContext>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));

        services.AddDbContext<GroupStatus.DatabaseContext>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));
    }

    [Fact]
    public async Task TestUpgrade()
    {
        SplitOutboxEmail.GroupType groupTypeBeforeUpgrade = new()
        {
            Id = 1,
            Name = "Dienstgruppe"
        };
        SplitOutboxEmail.Group beforeUpgrade = new()
        {
            Id = 1,
            GroupTypeId = 1,
            Name = "Admin"
        };
        GroupStatus.GroupType expectedGroupType = new()
        {
            Id = 1,
            Name = "Dienstgruppe",
            DeletionTime = default
        };
        GroupStatus.GroupStatus expectedIntermediateStatus = new()
        {
            Id = 1,
            Name = "default",
            DeletionTime = default
        };
        GroupStatus.Group expected = new()
        {
            Id = 1,
            GroupTypeId = 1,
            GroupType = expectedGroupType,
            GroupStatusId = 1,
            GroupStatus = expectedIntermediateStatus,
            Name = "Admin",
            DeletionTime = default
        };

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("SplitOutboxEmail");

        // Add test data
        splitOutboxEmail.GroupTypes.Add(groupTypeBeforeUpgrade);
        splitOutboxEmail.Groups.Add(beforeUpgrade);
        await splitOutboxEmail.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("GroupStatus");

        // Verify that data has been migrated as expected
        GroupStatus.GroupType groupType = await groupStatus.GroupTypes.SingleAsync();
        Assert.Equivalent(expectedGroupType, groupType);
        GroupStatus.GroupStatus intermediateStatus = await groupStatus.GroupStatuses.SingleAsync();
        Assert.Equivalent(expectedIntermediateStatus, intermediateStatus);
        GroupStatus.Group group = await groupStatus.Groups.Include(x => x.GroupType).Include(x => x.GroupStatus).SingleAsync();
        Assert.Equivalent(expected, group);
    }

    [Fact]
    public async Task TestDowngrade()
    {
        GroupStatus.GroupType groupTypeBeforeDowngrade = new()
        {
            Id = 1,
            Name = "Dienstgruppe"
        };
        GroupStatus.GroupStatus groupStatusBeforeDowngrade = new()
        {
            Id = 1,
            Name = "active"
        };
        GroupStatus.Group groupBeforeDowngrade = new()
        {
            Id = 1,
            GroupTypeId = 1,
            GroupStatusId = 1,
            Name = "Admin"
        };
        SplitOutboxEmail.GroupType expectedGroupType = new()
        {
            Id = 1,
            Name = "Dienstgruppe"
        };
        SplitOutboxEmail.Group expected = new()
        {
            Id = 1,
            GroupTypeId = 1,
            GroupType = expectedGroupType,
            Name = "Admin"
        };

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("GroupStatus");

        groupStatus.GroupTypes.Add(groupTypeBeforeDowngrade);
        groupStatus.GroupStatuses.Add(groupStatusBeforeDowngrade);
        groupStatus.Groups.Add(groupBeforeDowngrade);
        await groupStatus.SaveChangesAsync();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("SplitOutboxEmail");

        // Verify that data has been rolled back as expected
        SplitOutboxEmail.GroupType groupType = await splitOutboxEmail.GroupTypes.SingleAsync();
        Assert.Equivalent(expectedGroupType, groupType);
        SplitOutboxEmail.Group group = await splitOutboxEmail.Groups.SingleAsync();
        Assert.Equivalent(expected, group);
    }
}
