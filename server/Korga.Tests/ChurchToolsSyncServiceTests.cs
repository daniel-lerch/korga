using ChurchTools;
using ChurchTools.Model;
using Korga.ChurchTools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using DbGroupRole = Korga.ChurchTools.Entities.GroupRole;
using DbGroupType = Korga.ChurchTools.Entities.GroupType;

namespace Korga.Tests;

public class ChurchToolsSyncServiceTests : DatabaseTestBase
{
    private readonly FakeChurchToolsApi churchTools;
    private readonly ChurchToolsSyncService syncService;
    private readonly DateTime deletionTime = DateTime.Parse("2023-09-04 23:41:00");

    public ChurchToolsSyncServiceTests()
    {
        churchTools = (FakeChurchToolsApi)serviceScope.ServiceProvider.GetRequiredService<IChurchToolsApi>();
        syncService = serviceScope.ServiceProvider.GetRequiredService<ChurchToolsSyncService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IChurchToolsApi, FakeChurchToolsApi>();
        services.AddTransient<ChurchToolsSyncService>();
    }

    [Fact]
    public async Task TestIntialSync()
    {
        // Initialize database
        await databaseContext.Database.MigrateAsync();

        DbGroupType[] expectedGroupTypes =
        [
            new(1, "Kleingruppe"),
            new(2, "Dienst"),
        ];
        DbGroupRole[] expectedGroupRoles =
        [
            new(8, 1, "Teilnehmer") { GroupType = expectedGroupTypes[0] },
            new(9, 1, "Leiter") { GroupType = expectedGroupTypes[0] },
            new(15, 2, "Mitarbeiter") { GroupType = expectedGroupTypes[1] },
            new(16, 2, "Leiter") { GroupType = expectedGroupTypes[1] },
        ];

        churchTools.PersonMasterdata.Roles =
        [
            new(8, 1, "Teilnehmer", 0),
            new(9, 1, "Leiter", 0),
            new(15, 2, "Mitarbeiter", 0),
            new(16, 2, "Leiter", 0)
        ];
        churchTools.PersonMasterdata.GroupTypes =
        [
            new(1, "Kleingruppe", 0),
            new(2, "Dienst", 0)
        ];

        await syncService.Execute(CancellationToken.None);

        Assert.Equivalent(expectedGroupTypes, await databaseContext.GroupTypes.ToArrayAsync());
        Assert.Equivalent(expectedGroupRoles, await databaseContext.GroupRoles.ToArrayAsync());
    }

    [Fact]
    public async Task TestUpdate()
    {
        await InitializeSampleDataset();

        DbGroupType[] expectedGroupTypes =
        [
            new(1, "Kleingruppe"),
            new(2, "Dienst"),
        ];
        DbGroupRole[] expectedGroupRoles =
        [
            new(8, 1, "Teilnehmer*in") { GroupType = expectedGroupTypes[0] },
            new(9, 1, "Leiter*in") { GroupType = expectedGroupTypes[0] },
            new(15, 2, "Mitarbeiter*in") { GroupType = expectedGroupTypes[1] },
            new(16, 2, "Leiter*in") { GroupType = expectedGroupTypes[1] },
        ];

        churchTools.PersonMasterdata.Roles = [
            new(8, 1, "Teilnehmer*in", 0),
            new(9, 1, "Leiter*in", 0),
            new(15, 2, "Mitarbeiter*in", 0),
            new(16, 2, "Leiter*in", 0)
        ];
        churchTools.PersonMasterdata.GroupTypes = [
            new(1, "Kleingruppe", 0),
            new(2, "Dienst", 0)
        ];

        await syncService.Execute(CancellationToken.None);

        Assert.Equivalent(expectedGroupTypes, await databaseContext.GroupTypes.ToArrayAsync());
        Assert.Equivalent(expectedGroupRoles, await databaseContext.GroupRoles.ToArrayAsync());
    }

    [Fact]
    public async Task TestDelete()
    {
        await InitializeSampleDataset();

        churchTools.PersonMasterdata.Roles = [
            new(8, 1, "Teilnehmer", 0),
            new(9, 1, "Leiter", 0),
        ];
        churchTools.PersonMasterdata.GroupTypes = [
            new(1, "Kleingruppe", 0),
        ];

        await syncService.Execute(CancellationToken.None);

        var groupTypes = await databaseContext.GroupTypes.ToArrayAsync();
        Assert.Equal(2, groupTypes.Length);
        Assert.Equal(default, groupTypes[0].DeletionTime);
        Assert.NotEqual(default, groupTypes[1].DeletionTime);

        var groupRoles = await databaseContext.GroupRoles.ToArrayAsync();
        Assert.Equal(4, groupRoles.Length);
        Assert.Equal(default, groupRoles[0].DeletionTime);
        Assert.Equal(default, groupRoles[1].DeletionTime);
        Assert.NotEqual(default, groupRoles[2].DeletionTime);
        Assert.NotEqual(default, groupRoles[3].DeletionTime);
    }

    [Fact]
    public async Task TestUndoDelete()
    {
        await InitializePartiallyDeletedDataset();

        DbGroupType[] expectedGroupTypes =
        [
            new(1, "Kleingruppe"),
            new(2, "Dienst"),
        ];
        DbGroupRole[] expectedGroupRoles =
        [
            new(8, 1, "Teilnehmer") { GroupType = expectedGroupTypes[0] },
            new(9, 1, "Leiter") { GroupType = expectedGroupTypes[0] },
            new(15, 2, "Mitarbeiter") { GroupType = expectedGroupTypes[1] },
            new(16, 2, "Leiter") { GroupType = expectedGroupTypes[1] },
        ];

        churchTools.PersonMasterdata.Roles =
        [
            new(8, 1, "Teilnehmer", 0),
            new(9, 1, "Leiter", 0),
            new(15, 2, "Mitarbeiter", 0),
            new(16, 2, "Leiter", 0)
        ];
        churchTools.PersonMasterdata.GroupTypes =
        [
            new(1, "Kleingruppe", 0),
            new(2, "Dienst", 0)
        ];

        await syncService.Execute(CancellationToken.None);

        Assert.Equivalent(expectedGroupTypes, await databaseContext.GroupTypes.ToArrayAsync());
        Assert.Equivalent(expectedGroupRoles, await databaseContext.GroupRoles.ToArrayAsync());
    }

    [Fact]
    public async Task TestStillDeleted()
    {
        await InitializePartiallyDeletedDataset();

        churchTools.PersonMasterdata.Roles = [
            new(8, 1, "Teilnehmer", 0),
            new(9, 1, "Leiter", 0),
        ];
        churchTools.PersonMasterdata.GroupTypes = [
            new(1, "Kleingruppe", 0),
        ];

        await syncService.Execute(CancellationToken.None);

        var groupTypes = await databaseContext.GroupTypes.ToArrayAsync();
        Assert.Equal(2, groupTypes.Length);
        Assert.Equal(default, groupTypes[0].DeletionTime);
        Assert.Equal(deletionTime, groupTypes[1].DeletionTime);

        var groupRoles = await databaseContext.GroupRoles.ToArrayAsync();
        Assert.Equal(4, groupRoles.Length);
        Assert.Equal(default, groupRoles[0].DeletionTime);
        Assert.Equal(default, groupRoles[1].DeletionTime);
        Assert.Equal(deletionTime, groupRoles[2].DeletionTime);
        Assert.Equal(deletionTime, groupRoles[3].DeletionTime);
    }

    private async ValueTask InitializeSampleDataset()
    {
        // Initialize database
        await databaseContext.Database.MigrateAsync();

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
        await databaseContext.SaveChangesAsync();
    }

    private async ValueTask InitializePartiallyDeletedDataset()
    {
        // Initialize database
        await databaseContext.Database.MigrateAsync();

        databaseContext.GroupTypes.AddRange(
        [
            new(1, "Kleingruppe"),
            new(2, "Dienst") { DeletionTime = deletionTime },
        ]);
        databaseContext.GroupRoles.AddRange(
        [
            new(8, 1, "Teilnehmer"),
            new(9, 1, "Leiter"),
            new(15, 2, "Mitarbeiter") { DeletionTime = deletionTime },
            new(16, 2, "Leiter") { DeletionTime = deletionTime },
        ]);
        await databaseContext.SaveChangesAsync();
    }

    private class FakeChurchToolsApi : IChurchToolsApi
    {
        public Login? User { get; set; }
        public PersonMasterdata PersonMasterdata { get; } = new();
        public List<Person> People { get; } = [];
        public List<Group> Groups { get; } = [];
        public List<GroupMember> GroupMembers { get; } = [];

        public void Dispose() { }

        public ValueTask<List<Group>> GetGroups(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<List<Group>> GetGroups(IEnumerable<int> groupStatuses, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Groups);
        }

        public ValueTask<List<GroupMember>> GetGroupMembers(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(GroupMembers);
        }

        public ValueTask<List<Person>> GetPeople(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(People);
        }

        public ValueTask<Person> GetPerson(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Person> GetPerson(int personId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetPersonLoginToken(int personId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<PersonMasterdata> GetPersonMasterdata(CancellationToken cancellationToken = default)
        {
            if (PersonMasterdata == null)
                return ValueTask.FromException<PersonMasterdata>(new InvalidDataException());
            else
                return ValueTask.FromResult(PersonMasterdata);
        }

        public ValueTask<List<Service>> GetServices(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Service> GetService(int serviceId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<List<ServiceGroup>> GetServiceGroups(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<List<Event>> GetEvents(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<GlobalPermissions> GetGlobalPermissions(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
