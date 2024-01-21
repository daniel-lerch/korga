using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests.Migrations;

public class PersonFilterTreeMigrationTests : MigrationTestBase<GroupMemberStatus.DatabaseContext, PersonFilterTree.DatabaseContext>
{

    [Fact]
    public async Task TestUpgrade()
    {
        GroupMemberStatus.Status status = new()
        {
            Id = 1,
            Name = "unknown",
        };
        GroupMemberStatus.GroupStatus groupStatus = new()
        {
            Id = 1,
            Name = "active",
        };
        GroupMemberStatus.GroupType groupType = new()
        {
            Id = 1,
            Name = "Kleingruppe",
        };
        GroupMemberStatus.Group group = new()
        {
            Id = 1,
            Name = "Hauskreis",
            GroupTypeId = 1,
            GroupStatusId = 1,
        };
        GroupMemberStatus.Person person = new()
        {
            Id = 1,
            StatusId = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max.mustermann@example.org",
        };
        GroupMemberStatus.DistributionList distToStatus = new()
        {
            Id = 1,
            Alias = "unknown",
        };
        GroupMemberStatus.StatusFilter statusFilter = new()
        {
            Id = 1,
            StatusId = 1,
            DistributionListId = 1,
        };
        GroupMemberStatus.DistributionList distToNobody = new()
        {
            Id = 2,
            Alias = "nobody"
        };
        GroupMemberStatus.DistributionList distToGroupAndSinglePerson = new()
        {
            Id = 3,
            Alias = "kleingruppen"
        };
        GroupMemberStatus.GroupFilter groupFilter = new()
        {
            Id = 2,
            GroupId = 1,
            DistributionListId = 3,
        };
        GroupMemberStatus.SinglePerson singlePerson = new()
        {
            Id = 3,
            PersonId = 1,
            DistributionListId = 3,
        };
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of last migration before the one to test
        await migrator.MigrateAsync("GroupMemberStatus");

        // Add test data
        before.Status.Add(status);
        before.GroupStatuses.Add(groupStatus);
        before.GroupTypes.Add(groupType);
        before.Groups.Add(group);
        before.People.Add(person);
        before.DistributionLists.Add(distToStatus);
        before.PersonFilters.Add(statusFilter);
        before.DistributionLists.Add(distToNobody);
        before.DistributionLists.Add(distToGroupAndSinglePerson);
        before.PersonFilters.Add(groupFilter);
        before.PersonFilters.Add(singlePerson);
        await before.SaveChangesAsync();

        // Run migration at test
        await migrator.MigrateAsync("PersonFilterTree");

        // Verify that data has been migrated as expected
        List<PersonFilterTree.DistributionList> distributionLists = await after.DistributionLists.ToListAsync();
        Assert.Equal(3, distributionLists.Count);

        var or0 = (await after.PersonFilters.Include(f => f.Children).SingleAsync(f => f.Id == distributionLists[0].PermittedRecipientsId)) as PersonFilterTree.LogicalOr;
        Assert.NotNull(or0);
        Assert.NotNull(or0.Children);
        var or0_filter0 = or0.Children.Single() as PersonFilterTree.StatusFilter;
        Assert.NotNull(or0_filter0);
        Assert.Equal(1, or0_filter0.StatusId);

        Assert.Null(distributionLists[1].PermittedRecipientsId);

        var or2 = (await after.PersonFilters.Include(f => f.Children).SingleAsync(f => f.Id == distributionLists[2].PermittedRecipientsId)) as PersonFilterTree.LogicalOr;
        Assert.NotNull (or2);
        Assert.NotNull(or2.Children);
        var or2_filter0 = or2.Children.Single(f => f is PersonFilterTree.GroupFilter) as PersonFilterTree.GroupFilter;
        Assert.NotNull(or2_filter0);
        Assert.Equal(1, or2_filter0.GroupId);
        var or2_filter1 = or2.Children.Single(f => f is PersonFilterTree.SinglePerson) as PersonFilterTree.SinglePerson;
        Assert.NotNull(or2_filter1);
        Assert.Equal(1, or2_filter1.PersonId);
    }

    [Fact]
    public async Task TestDowngrade()
    {

        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("PersonFilterTree");

        // Add test data
        await after.SaveChangesAsync();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("GroupMemberStatus");
    }
}
