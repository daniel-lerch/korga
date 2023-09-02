﻿using Korga.ChurchTools;
using Korga.ChurchTools.Api;
using Korga.Server.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IArchivable = Korga.ChurchTools.Entities.IArchivable;
using DbDepartmentMember = Korga.ChurchTools.Entities.DepartmentMember;
using DbGroupMember = Korga.ChurchTools.Entities.GroupMember;
using Microsoft.Extensions.Logging;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncService
{
	private readonly ILogger<ChurchToolsSyncService> logger;
	private readonly DatabaseContext database;
	private readonly IChurchToolsApi churchTools;

	public ChurchToolsSyncService(ILogger<ChurchToolsSyncService> logger, DatabaseContext database, IChurchToolsApi churchTools)
	{
		this.logger = logger;
		this.database = database;
		this.churchTools = churchTools;
	}

	public async ValueTask Execute(CancellationToken cancellationToken)
	{
		PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(cancellationToken);

		await SynchronizeArchivable(personMasterdata.GroupTypes, database.GroupTypes, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, cancellationToken);

		await SynchronizeArchivable(personMasterdata.GroupStatuses, database.GroupStatuses, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, cancellationToken);

		await SynchronizeArchivable(personMasterdata.Roles, database.GroupRoles, x => new(x.Id, x.GroupTypeId, x.Name), (x, y) => { y.GroupTypeId = x.GroupTypeId; y.Name = x.Name; }, cancellationToken);

		await SynchronizeArchivable(personMasterdata.Departments, database.Departments, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, cancellationToken);
		
		await SynchronizeArchivable(personMasterdata.Statuses, database.Status, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, cancellationToken);

		await Task.WhenAll(
			SynchronizeGroups(personMasterdata.GroupStatuses.Select(x => x.Id), cancellationToken),
			SynchronizePeople(cancellationToken)
		);

		await SynchronizeGroupMembers(cancellationToken);
	}

	private async Task SynchronizeGroups(IEnumerable<int> groupStatuses, CancellationToken cancellationToken)
	{
		await SynchronizeArchivable(
			await churchTools.GetGroups(groupStatuses, cancellationToken),
			database.Groups,
			x => new(x.Id, x.GroupTypeId, x.GroupStatusId, x.Name),
			(response, entity) =>
			{
				entity.GroupTypeId = response.GroupTypeId;
				entity.Name = response.Name;
			},
			cancellationToken
		);
	}

	private async Task SynchronizePeople(CancellationToken cancellationToken)
	{
        List<Person> people = await churchTools.GetPeople(cancellationToken);

        await SynchronizeArchivable(
            people,
            database.People,
			x => new(x.Id, x.StatusId, x.FirstName, x.LastName, x.Email),
			(response, entity) =>
			{
				entity.StatusId = response.StatusId;
				entity.FirstName = response.FirstName;
				entity.LastName = response.LastName;
				entity.Email = response.Email;
			},
			cancellationToken
		);

		List<DbDepartmentMember> departmentMemberships = 
			people.SelectMany(x => x.DepartmentIds.Select(y => new DbDepartmentMember { PersonId = x.Id, DepartmentId = y })).ToList();

		await Synchronize<DbDepartmentMember, DbDepartmentMember, long>(
			departmentMemberships,
			database.DepartmentMembers,
			await database.DepartmentMembers.OrderBy(x => x.PersonId).ThenBy(x => x.DepartmentId).ToListAsync(cancellationToken),
			x => x,
			(x, y) => { },
			cancellationToken
		);
	}

	private async ValueTask SynchronizeGroupMembers(CancellationToken cancellationToken)
	{
		await Synchronize<GroupMember, DbGroupMember, long>(
			await churchTools.GetGroupMembers(cancellationToken),
			database.GroupMembers,
			await database.GroupMembers.OrderBy(x => x.PersonId).ThenBy(x => x.GroupId).ToListAsync(cancellationToken),
			x => new() { PersonId = x.PersonId, GroupId = x.GroupId, GroupRoleId = x.GroupTypeRoleId },
			(response, entity) => entity.GroupRoleId = response.GroupTypeRoleId,
			cancellationToken
		);
	}

    private async ValueTask Synchronize<TSrc, TDest, TKey>(List<TSrc> apiResponses, DbSet<TDest> cachedSet, List<TDest> cachedValues, Func<TSrc, TDest> convert, Action<TSrc, TDest> update, CancellationToken cancellationToken)
		where TSrc : IIdentifiable<TKey>
		where TDest : class, IIdentifiable<TKey>
		where TKey : IComparable<TKey>
	{
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));

		foreach ((TSrc? response, TDest? entity) in apiResponses.ContrastWith<TSrc, TDest, TKey>(cachedValues))
		{
			if (response == null)
				cachedSet.Remove(entity!);
			else if (entity == null)
				cachedSet.Add(convert(response));
			else
				update(response, entity);
		}

        int rowsAffected = await database.SaveChangesAsync(cancellationToken);
		LogChanges(rowsAffected, cachedSet);
	}

    private async ValueTask SynchronizeArchivable<TSrc, TDest>(List<TSrc> apiResponses, DbSet<TDest> cachedSet, Func<TSrc, TDest> convert, Action<TSrc, TDest> update, CancellationToken cancellationToken)
		where TSrc : IIdentifiable<int>
		where TDest : class, IIdentifiable<int>, IArchivable
	{
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<TDest> cachedValues = await cachedSet.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		foreach ((TSrc? response, TDest? entity) in apiResponses.ContrastWith<TSrc, TDest, int>(cachedValues))
		{
			if (response == null)
				// TODO: In case of a paginated API: Make sure this item really doesn't exist anymore before archiving it
				entity!.DeletionTime = DateTime.UtcNow;
			else if (entity == null)
				cachedSet.Add(convert(response));
			else
				update(response, entity);
		}

        int rowsAffected = await database.SaveChangesAsync(cancellationToken);
		LogChanges(rowsAffected, cachedSet);
    }

	private void LogChanges<T>(int rowsAffected, DbSet<T> table) where T : class
	{
        if (rowsAffected > 0)
            logger.LogInformation("Updated {Count} {EntityDisplayName} entities", rowsAffected, table.EntityType.DisplayName());
        else
            logger.LogDebug("No changes for {EntityDisplayName} entities", table.EntityType.DisplayName());
    }
}
