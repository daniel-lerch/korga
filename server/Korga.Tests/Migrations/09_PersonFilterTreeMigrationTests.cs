using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Tests.Migrations;

public class PersonFilterListMigrationTests : MigrationTestBase<GroupMemberStatus.DatabaseContext, PersonFilterList.DatabaseContext>
{
    public PersonFilterListMigrationTests(ITestOutputHelper testOutput) : base(testOutput) { }

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
        await migrator.MigrateAsync("GroupMemberStatus", TestContext.Current.CancellationToken);

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
        await before.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Run migration at test
        await migrator.MigrateAsync("PersonFilterList", TestContext.Current.CancellationToken);

        // Verify that data has been migrated as expected
        List<PersonFilterList.DistributionList> distributionLists = await after.DistributionLists.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(3, distributionLists.Count);

        var fl0 = await after.PersonFilterLists.Include(fl => fl.Filters).SingleAsync(fl => fl.Id == distributionLists[0].PermittedRecipientsId, TestContext.Current.CancellationToken);
        Assert.NotNull(fl0);
        Assert.NotNull(fl0.Filters);
        var or0_filter0 = fl0.Filters.Single() as PersonFilterList.StatusFilter;
        Assert.NotNull(or0_filter0);
        Assert.Equal(1, or0_filter0.StatusId);

        Assert.Null(distributionLists[1].PermittedRecipientsId);

        var fl2 = await after.PersonFilterLists.Include(fl => fl.Filters).SingleAsync(fl => fl.Id == distributionLists[2].PermittedRecipientsId, TestContext.Current.CancellationToken);
        Assert.NotNull(fl2);
        Assert.NotNull(fl2.Filters);
        Assert.Equal(2, fl2.Filters.Count);
        var or2_filter0 = fl2.Filters.Single(f => f is PersonFilterList.GroupFilter) as PersonFilterList.GroupFilter;
        Assert.NotNull(or2_filter0);
        Assert.Equal(1, or2_filter0.GroupId);
        var or2_filter1 = fl2.Filters.Single(f => f is PersonFilterList.SinglePerson) as PersonFilterList.SinglePerson;
        Assert.NotNull(or2_filter1);
        Assert.Equal(1, or2_filter1.PersonId);
    }

    [Fact]
    public async Task TestDowngrade()
    {
        PersonFilterList.Status status = new()
        {
            Id = 1,
            Name = "unknown",
        };
        PersonFilterList.GroupStatus groupStatus = new()
        {
            Id = 1,
            Name = "active",
        };
        PersonFilterList.GroupType groupType = new()
        {
            Id = 1,
            Name = "Kleingruppe",
        };
        PersonFilterList.Group group = new()
        {
            Id = 1,
            Name = "Hauskreis",
            GroupTypeId = 1,
            GroupStatusId = 1,
        };
        PersonFilterList.Person person = new()
        {
            Id = 1,
            StatusId = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max.mustermann@example.org",
        };
        PersonFilterList.PersonFilterList statusFilterList = new()
        {
            Id = 1,
            Filters = [new PersonFilterList.StatusFilter { Id = 1, StatusId = 1 }]
        };
        PersonFilterList.DistributionList distToStatus = new()
        {
            Id = 1,
            Alias = "unknown",
            PermittedRecipientsId = 1,
        };
        PersonFilterList.DistributionList distToNobody = new()
        {
            Id = 2,
            Alias = "nobody",
            PermittedRecipientsId = null,
        };
        PersonFilterList.PersonFilterList groupAndSinglePersonFilterList = new()
        {
            Id = 2,
            Filters = [
                new PersonFilterList.GroupFilter { Id = 2, GroupId = 1 },
                new PersonFilterList.SinglePerson { Id = 3, PersonId = 1 },
                new PersonFilterList.GroupTypeFilter { Id = 4, GroupTypeId = 1 },
            ]
        };
        PersonFilterList.DistributionList distToGroupAndSinglePerson = new()
        {
            Id = 3,
            Alias = "kleingruppen",
            PermittedRecipientsId = 2,
        };
        PersonFilterList.PersonFilterList danglingStatusFilterList = new()
        {
            Id = 4,
            Filters = [new PersonFilterList.StatusFilter { Id = 5, StatusId = 1 }]
        };
        IMigrator migrator = databaseContext.GetInfrastructure().GetRequiredService<IMigrator>();

        // Create database schema of the migration to test
        await migrator.MigrateAsync("PersonFilterList", TestContext.Current.CancellationToken);

        // Add test data
        after.Status.Add(status);
        after.GroupStatuses.Add(groupStatus);
        after.GroupTypes.Add(groupType);
        after.Groups.Add(group);
        after.People.Add(person);
        after.PersonFilterLists.Add(statusFilterList);
        after.DistributionLists.Add(distToStatus);
        after.DistributionLists.Add(distToNobody);
        after.PersonFilterLists.Add(groupAndSinglePersonFilterList);
        after.DistributionLists.Add(distToGroupAndSinglePerson);
        after.PersonFilterLists.Add(danglingStatusFilterList);
        await after.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Reset change tracker before upgrading the schema again to avoid caching
        after.ChangeTracker.Clear();

        // Migrate to migration before the one to test and thereby revert it
        await migrator.MigrateAsync("GroupMemberStatus", TestContext.Current.CancellationToken);

        // Verify that data has been migrated as expected
        List<GroupMemberStatus.DistributionList> distributionLists = await before.DistributionLists.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(3, distributionLists.Count);

        List<GroupMemberStatus.PersonFilter> personFilters = await before.PersonFilters.ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(3, personFilters.Count);

        // Upgrade database again to verify rollback worked
        await migrator.MigrateAsync("PersonFilterList", TestContext.Current.CancellationToken);
    }
}
