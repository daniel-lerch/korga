using Korga.ChurchTools.Entities;
using Korga.Server.ChurchTools.Api;
using Korga.Server.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncHostedService : RepeatedExecutionService
{
	private readonly IServiceProvider serviceProvider;

	public ChurchToolsSyncHostedService(ILogger<ChurchToolsSyncHostedService> logger, IServiceProvider serviceProvider) : base(logger)
	{
		this.serviceProvider = serviceProvider;

		Interval = TimeSpan.FromSeconds(30);
	}

	protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
	{
		using IServiceScope serviceScope = serviceProvider.CreateScope();
		DatabaseContext database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
		IChurchToolsApiService churchTools = serviceScope.ServiceProvider.GetRequiredService<IChurchToolsApiService>();

		PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(stoppingToken);
		await SyncGroupTypes(database, personMasterdata.GroupTypes, stoppingToken);
		await SyncGroupRoles(database, personMasterdata.Roles, stoppingToken);
		await SyncStatuses(database, personMasterdata.Statuses, stoppingToken);
	}

	private async ValueTask SyncGroupTypes(DatabaseContext database, List<PersonMasterdata.GroupType> groupTypes, CancellationToken cancellationToken)
	{
		groupTypes.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<GroupType> dbGroupTypes = await database.GroupTypes.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		new GroupTypeSynchronizer(database.GroupTypes).Sync(groupTypes, dbGroupTypes);
		await database.SaveChangesAsync(cancellationToken);
	}

	private async ValueTask SyncGroupRoles(DatabaseContext database, List<PersonMasterdata.Role> groupRoles, CancellationToken cancellationToken)
	{
		groupRoles.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<GroupRole> dbGroupRoles = await database.GroupRoles.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		new GroupRoleSynchronizer(database.GroupRoles).Sync(groupRoles, dbGroupRoles);
		await database.SaveChangesAsync(cancellationToken);
	}

	private async ValueTask SyncStatuses(DatabaseContext database, List<PersonMasterdata.Status> statuses, CancellationToken cancellationToken)
	{
		statuses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<Status> dbStatuses = await database.Status.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		new StatusSynchronizer(database.Status).Sync(statuses, dbStatuses);
		await database.SaveChangesAsync(cancellationToken);
	}


	private class GroupTypeSynchronizer : CollectionToDbSetSynchronizer<PersonMasterdata.GroupType, GroupType, int>
	{
		public GroupTypeSynchronizer(DbSet<GroupType> destinationSet) : base(destinationSet) { }

		protected override GroupType Convert(PersonMasterdata.GroupType src)
		{
			return new(src.Id, src.Name);
		}

		protected override void Update(PersonMasterdata.GroupType src, GroupType dest)
		{
			dest.Name = src.Name;
		}
	}

	private class GroupRoleSynchronizer : CollectionToDbSetSynchronizer<PersonMasterdata.Role, GroupRole, int>
	{
		public GroupRoleSynchronizer(DbSet<GroupRole> destinationSet) : base(destinationSet) { }

		protected override GroupRole Convert(PersonMasterdata.Role src)
		{
			return new(src.Id, src.GroupTypeId, src.Name);
		}

		protected override void Update(PersonMasterdata.Role src, GroupRole dest)
		{
			dest.Name = src.Name;
			dest.GroupTypeId = src.GroupTypeId;
		}
	}

	private class StatusSynchronizer : CollectionToDbSetSynchronizer<PersonMasterdata.Status, Status, int>
	{
		public StatusSynchronizer(DbSet<Status> destinationSet) : base(destinationSet) { }

		protected override Status Convert(PersonMasterdata.Status src)
		{
			return new(src.Id, src.Name);
		}

		protected override void Update(PersonMasterdata.Status src, Status dest)
		{
			dest.Name = src.Name;
		}
	}
}
