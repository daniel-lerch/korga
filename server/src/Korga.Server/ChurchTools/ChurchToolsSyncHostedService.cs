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

		new GroupTypeSynchronizer(database).Sync(groupTypes, dbGroupTypes);
		await database.SaveChangesAsync(cancellationToken);
	}

	private async ValueTask SyncGroupRoles(DatabaseContext database, List<PersonMasterdata.Role> groupRoles, CancellationToken cancellationToken)
	{
		groupRoles.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<GroupRole> dbGroupRoles = await database.GroupRoles.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		new GroupRoleSynchronizer(database).Sync(groupRoles, dbGroupRoles);
		await database.SaveChangesAsync(cancellationToken);
	}

	private async ValueTask SyncStatuses(DatabaseContext database, List<PersonMasterdata.Status> statuses, CancellationToken cancellationToken)
	{
		statuses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<Status> dbStatuses = await database.Status.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		new StatusSynchronizer(database).Sync(statuses, dbStatuses);
		await database.SaveChangesAsync(cancellationToken);
	}


	private class GroupTypeSynchronizer : CollectionSynchronizer<PersonMasterdata.GroupType, GroupType, int>
	{
		private readonly DatabaseContext database;

		public GroupTypeSynchronizer(DatabaseContext database)
		{
			this.database = database;
		}

		protected override void Add(PersonMasterdata.GroupType src)
		{
			database.GroupTypes.Add(new GroupType(src.Id, src.Name));
		}

		protected override int GetDstKey(GroupType dest) => dest.Id;

		protected override int GetSrcKey(PersonMasterdata.GroupType src) => src.Id;

		protected override void Remove(GroupType dest)
		{
			database.GroupTypes.Remove(dest);
		}

		protected override void Update(PersonMasterdata.GroupType src, GroupType dest)
		{
			dest.Name = src.Name;
		}
	}

	private class GroupRoleSynchronizer : CollectionSynchronizer<PersonMasterdata.Role, GroupRole, int>
	{
		private readonly DatabaseContext database;

		public GroupRoleSynchronizer(DatabaseContext database)
		{
			this.database = database;
		}

		protected override void Add(PersonMasterdata.Role src)
		{
			database.GroupRoles.Add(new(src.Id, src.GroupTypeId, src.Name));
		}

		protected override int GetDstKey(GroupRole dest) => dest.Id;

		protected override int GetSrcKey(PersonMasterdata.Role src) => src.Id;

		protected override void Remove(GroupRole dest)
		{
			database.GroupRoles.Remove(dest);
		}

		protected override void Update(PersonMasterdata.Role src, GroupRole dest)
		{
			dest.Name = src.Name;
			dest.GroupTypeId = src.GroupTypeId;
		}
	}

	private class StatusSynchronizer : CollectionSynchronizer<PersonMasterdata.Status, Status, int>
	{
		private readonly DatabaseContext database;

		public StatusSynchronizer(DatabaseContext database)
		{
			this.database = database;
		}

		protected override void Add(PersonMasterdata.Status src)
		{
			database.Status.Add(new(src.Id, src.Name));
		}

		protected override int GetDstKey(Status dest) => dest.Id;

		protected override int GetSrcKey(PersonMasterdata.Status src) => src.Id;

		protected override void Remove(Status dest)
		{
			database.Status.Remove(dest);
		}

		protected override void Update(PersonMasterdata.Status src, Status dest)
		{
			dest.Name = src.Name;
		}
	}
}
