using Korga.ChurchTools.Entities;
using Korga.EmailRelay.Entities;
using Korga.Server.EmailRelay;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests;

public class DistributionListServiceTests : DatabaseTestBase
{
    private readonly DistributionListService distributionListService;

    public DistributionListServiceTests()
    {
        distributionListService = serviceScope.ServiceProvider.GetRequiredService<DistributionListService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<DistributionListService>();
    }

    [Fact]
    public async Task TestGetPeopleRecursive_Or()
    {
        await InitializeSampleDataset();

        // Create person filter
        LogicalOr logicalOr = new();
        databaseContext.PersonFilters.Add(logicalOr);
        await databaseContext.SaveChangesAsync();

        ISet<Person> people = await distributionListService.GetPeopleRecursive(logicalOr);
        Assert.Empty(people);
    }

    [Fact]
    public async Task TestGetPeopleRecursive_Person()
    {
        await InitializeSampleDataset();

        // Create person filter
        SinglePerson singlePerson = new() { PersonId = 1 };
        databaseContext.PersonFilters.Add(singlePerson);
        await databaseContext.SaveChangesAsync();

        ISet<Person> people = await distributionListService.GetPeopleRecursive(singlePerson);
        Person actual = Assert.Single(people);
        Assert.Equal(1, actual.Id);
    }

    [Fact]
    public async Task TestGetPeopleRecursive_Status()
    {
        await InitializeSampleDataset();

        // Create person filter
        StatusFilter statusFilter = new() { StatusId = 3 };
        databaseContext.PersonFilters.Add(statusFilter);
        await databaseContext.SaveChangesAsync();

        ISet<Person> people = await distributionListService.GetPeopleRecursive(statusFilter);
        Assert.Equal(2, people.Count);
        Assert.Contains(people, p => p.Id == 1);
        Assert.Contains(people, p => p.Id == 2);
    }

    [Fact]
    public async Task TestGetPeopleRecursive_Status_Or_Status()
    {
        await InitializeSampleDataset();

        // Create person filter
        LogicalOr logicalOr = new()
        {
            Children = new[]
            {
                new StatusFilter { StatusId = 2 },
                new StatusFilter { StatusId = 3 },
            }
        };
        databaseContext.PersonFilters.Add(logicalOr);
        await databaseContext.SaveChangesAsync();

        ISet<Person> people = await distributionListService.GetPeopleRecursive(logicalOr);
        Assert.Equal(3, people.Count);
        Assert.Contains(people, p => p.Id == 1);
        Assert.Contains(people, p => p.Id == 2);
        Assert.Contains(people, p => p.Id == 3);
    }

    [Fact]
    public async Task TestGetPeopleRecursive_Group_And_Status_Empty()
    {
        await InitializeSampleDataset();

        // Create person filter
        LogicalAnd logicalAnd = new()
        {
            Children = new PersonFilter[]
            {
                new GroupFilter { GroupId = 1 },
                new StatusFilter { StatusId = 2 },
            }
        };
        databaseContext.PersonFilters.Add(logicalAnd);
        await databaseContext.SaveChangesAsync();

        ISet<Person> people = await distributionListService.GetPeopleRecursive(logicalAnd);
        Assert.Empty(people);
    }

    [Fact]
    public async Task TestGetPeopleRecursive_Group_And_Status()
    {
        await InitializeSampleDataset();

        // Create person filter
        LogicalAnd logicalAnd = new()
        {
            Children = new PersonFilter[]
            {
                new GroupFilter { GroupId = 2 },
                new StatusFilter { StatusId = 1 },
            }
        };
        databaseContext.PersonFilters.Add(logicalAnd);
        await databaseContext.SaveChangesAsync();

        ISet<Person> people = await distributionListService.GetPeopleRecursive(logicalAnd);
        Person actual = Assert.Single(people);
        Assert.Equal(4, actual.Id);
    }

    private async ValueTask InitializeSampleDataset()
    {
        // Initialize database
        //await databaseContext.Database.MigrateAsync();
        await databaseContext.Database.EnsureCreatedAsync();

        databaseContext.Status.AddRange(new Status[]
        {
            new(1, "Gast"),
            new(2, "Freund"),
            new(3, "Mitglied"),
        });
        databaseContext.GroupTypes.AddRange(new GroupType[]
        {
            new(1, "Kleingruppe"),
            new(2, "Dienst"),
        });
        databaseContext.GroupRoles.AddRange(new GroupRole[]
        {
            new(8, 1, "Teilnehmer"),
            new(9, 1, "Leiter"),
            new(15, 2, "Mitarbeiter"),
            new(16, 2, "Leiter"),
        });
        databaseContext.GroupStatuses.AddRange(new GroupStatus[]
        {
            new(1, "active"),
            new(2, "archived"),
        });
        databaseContext.People.AddRange(new Person[]
        {
            new(1, 3, "Markus", "Wiebe", "mwiebe@example.org"),
            new(2, 3, "Debora", "Wiebe", "debora.wiebe@example.org"),
            new(3, 2, "Mohammad", "Khamenei", "m.k@example.org"),
            new(4, 1, "Barbara", "Müller", "barbara_m@example.org"),
        });
        databaseContext.Groups.AddRange(new Group[]
        {
            new(1, 1, 1, "Admin"),
            new(2, 2, 1, "Kinderdienst"),
        });
        databaseContext.GroupMembers.AddRange(new GroupMember[]
        {
            new() { PersonId = 1, GroupId = 1, GroupRoleId = 9 },
            new() { PersonId = 2, GroupId = 2, GroupRoleId = 16 },
            new() { PersonId = 4, GroupId = 2, GroupRoleId = 15 },
        });
        await databaseContext.SaveChangesAsync();
    }
}
