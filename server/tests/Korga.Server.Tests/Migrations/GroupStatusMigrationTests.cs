﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests.Migrations;

public class GroupStatusMigrationTests : MigrationTestBase<SplitOutboxEmail.DatabaseContext, GroupStatus.DatabaseContext>
{
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
        before.GroupTypes.Add(groupTypeBeforeUpgrade);
        before.Groups.Add(beforeUpgrade);
        await before.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("GroupStatus");

        // Verify that data has been migrated as expected
        GroupStatus.GroupType groupType = await after.GroupTypes.SingleAsync();
        Assert.Equivalent(expectedGroupType, groupType);
        GroupStatus.GroupStatus intermediateStatus = await after.GroupStatuses.SingleAsync();
        Assert.Equivalent(expectedIntermediateStatus, intermediateStatus);
        GroupStatus.Group group = await after.Groups.Include(x => x.GroupType).Include(x => x.GroupStatus).SingleAsync();
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

        after.GroupTypes.Add(groupTypeBeforeDowngrade);
        after.GroupStatuses.Add(groupStatusBeforeDowngrade);
        after.Groups.Add(groupBeforeDowngrade);
        await after.SaveChangesAsync();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("SplitOutboxEmail");

        // Verify that data has been rolled back as expected
        SplitOutboxEmail.GroupType groupType = await before.GroupTypes.SingleAsync();
        Assert.Equivalent(expectedGroupType, groupType);
        SplitOutboxEmail.Group group = await before.Groups.SingleAsync();
        Assert.Equivalent(expected, group);
    }
}
