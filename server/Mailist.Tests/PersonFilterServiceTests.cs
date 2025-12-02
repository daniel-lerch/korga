using Mailist.ChurchTools.Entities;
using Mailist.Filters;
using Mailist.Filters.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class PersonFilterServiceTests : DatabaseTestBase
{
    private readonly PersonFilterService personFilterService;

    public PersonFilterServiceTests()
    {
        personFilterService = serviceScope.ServiceProvider.GetRequiredService<PersonFilterService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<PersonFilterService>();
    }

    [Fact]
    public async Task TestGetPeople_Empty()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create an empty person filter list
        PersonFilterList filterList = new();
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var people = await personFilterService.GetPeople(filterList.Id, TestContext.Current.CancellationToken);
        Assert.Empty(people);
    }

    [Fact]
    public async Task TestGetPeople_Person()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create person filter
        PersonFilterList filterList = new()
        {
            Filters = [new SinglePerson { PersonId = 1 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var people = await personFilterService.GetPeople(filterList.Id, TestContext.Current.CancellationToken);
        Person actual = Assert.Single(people);
        Assert.Equal(1, actual.Id);
    }

    [Fact]
    public async Task TestGetPeople_Status()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create status filter
        PersonFilterList filterList = new()
        {
            Filters = [new StatusFilter { StatusId = 3 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var people = await personFilterService.GetPeople(filterList.Id, TestContext.Current.CancellationToken);
        Assert.Equal(2, people.Count());
        Assert.Contains(people, p => p.Id == 1);
        Assert.Contains(people, p => p.Id == 2);
    }

    [Fact]
    public async Task TestGetPeople_Status_Status()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create person filter
        PersonFilterList filterList = new()
        {
            Filters =
            [
                new StatusFilter { StatusId = 2 },
                new StatusFilter { StatusId = 3 },
            ]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var people = await personFilterService.GetPeople(filterList.Id, TestContext.Current.CancellationToken);
        Assert.Equal(3, people.Count());
        Assert.Contains(people, p => p.Id == 1);
        Assert.Contains(people, p => p.Id == 2);
        Assert.Contains(people, p => p.Id == 3);
    }

    [Fact]
    public async Task TestGetPeople_Group()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create person filter
        PersonFilterList filterList = new()
        {
            Filters = [new GroupFilter { GroupId = 1 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var people = await personFilterService.GetPeople(filterList.Id, TestContext.Current.CancellationToken);
        Person actual = Assert.Single(people);
        Assert.Equal(1, actual.Id);
    }

    [Fact]
    public async Task TestAddFilter_Group_EmptyList()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);
        PersonFilterList filterList = new();
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted = await personFilterService.AddFilter(filterList.Id, new GroupFilter { GroupId = 1 });
        Assert.True(inserted);
    }

    [Fact]
    public async Task TestAddFilter_Group_NewFilter()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create sample filter
        PersonFilterList filterList = new()
        {
            Filters = [new GroupFilter { GroupId = 1 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted = await personFilterService.AddFilter(filterList.Id, new GroupFilter { GroupId = 2 });
        Assert.True(inserted);
    }

    [Fact]
    public async Task TestAddFilter_Group_Different_Lists()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        PersonFilterList filterList1 = new();
        PersonFilterList filterList2 = new();
        databaseContext.PersonFilterLists.AddRange(filterList1, filterList2);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted1 = await personFilterService.AddFilter(filterList1.Id, new GroupFilter { GroupId = 1 });
        Assert.True(inserted1);

        bool inserted2 = await personFilterService.AddFilter(filterList2.Id, new GroupFilter { GroupId = 1 });
        Assert.True(inserted2);
    }

    [Fact]
    public async Task TestAddFilter_Group_AlreadyExists()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create sample filter
        PersonFilterList filterList = new()
        {
            Filters = [new GroupFilter { GroupId = 1 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted = await personFilterService.AddFilter(filterList.Id, new GroupFilter { GroupId = 1 });
        Assert.False(inserted);
    }

    [Fact]
    public async Task TestAddFilter_GroupType_AlreadyExists()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create sample filter
        PersonFilterList filterList = new()
        {
            Filters = [new GroupTypeFilter { GroupTypeId = 1 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted = await personFilterService.AddFilter(filterList.Id, new GroupTypeFilter { GroupTypeId = 1 });
        Assert.False(inserted);
    }

    [Fact]
    public async Task TestAddFilter_SinglePerson_AlreadyExists()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create sample filter
        PersonFilterList filterList = new()
        {
            Filters = [new SinglePerson { PersonId = 1 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted = await personFilterService.AddFilter(filterList.Id, new SinglePerson { PersonId = 1 });
        Assert.False(inserted);
    }

    [Fact]
    public async Task TestAddFilter_Status_AlreadyExists()
    {
        await InitializeSampleDataset(TestContext.Current.CancellationToken);

        // Create sample filter
        PersonFilterList filterList = new()
        {
            Filters = [new StatusFilter { StatusId = 3 }]
        };
        databaseContext.PersonFilterLists.Add(filterList);
        await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        bool inserted = await personFilterService.AddFilter(filterList.Id, new StatusFilter { StatusId = 3 });
        Assert.False(inserted);
    }


    private async ValueTask InitializeSampleDataset(CancellationToken cancellationToken)
    {
        // Initialize database
        await databaseContext.Database.MigrateAsync(cancellationToken);

        databaseContext.Status.AddRange(
        [
            new(1, "Gast"),
            new(2, "Freund"),
            new(3, "Mitglied"),
        ]);
        databaseContext.GroupTypes.AddRange(
        [
            new(1, "Kleingruppe"),
            new(2, "Dienst"),
        ]);
        databaseContext.GroupRoles.AddRange(
        [
            new(8, 1, "Teilnehmer"),
            new(9, 1, "Leiter"),
            new(15, 2, "Mitarbeiter"),
            new(16, 2, "Leiter"),
        ]);
        databaseContext.GroupStatuses.AddRange(
        [
            new(1, "active"),
            new(2, "archived"),
        ]);
        databaseContext.People.AddRange(
        [
            new(1, 3, "Markus", "Wiebe", "mwiebe@example.org"),
            new(2, 3, "Debora", "Wiebe", "debora.wiebe@example.org"),
            new(3, 2, "Mohammad", "Khamenei", "m.k@example.org"),
            new(4, 1, "Barbara", "Müller", "barbara_m@example.org"),
        ]);
        databaseContext.Groups.AddRange(
        [
            new(1, 1, 1, "Admin"),
            new(2, 2, 1, "Kinderdienst"),
        ]);
        databaseContext.GroupMembers.AddRange(
        [
            new() { PersonId = 1, GroupId = 1, GroupRoleId = 9 },
            new() { PersonId = 2, GroupId = 2, GroupRoleId = 16 },
            new() { PersonId = 4, GroupId = 2, GroupRoleId = 15 },
        ]);
        await databaseContext.SaveChangesAsync(cancellationToken);
    }
}
