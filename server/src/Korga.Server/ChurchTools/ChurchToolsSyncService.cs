using Korga.ChurchTools;
using Korga.ChurchTools.Api;
using Korga.Server.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DbGroupMember = Korga.ChurchTools.Entities.GroupMember;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncService
{
	private readonly DatabaseContext database;
	private readonly IChurchToolsApi churchTools;

	public ChurchToolsSyncService(DatabaseContext database, IChurchToolsApi churchTools)
	{
		this.database = database;
		this.churchTools = churchTools;
	}

	public async ValueTask Execute(CancellationToken cancellationToken)
	{
		PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(cancellationToken);

		await Synchronize(personMasterdata.GroupTypes, database.GroupTypes, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, cancellationToken);

		await Synchronize(personMasterdata.Roles, database.GroupRoles, x => new(x.Id, x.GroupTypeId, x.Name), (x, y) => { y.GroupTypeId = x.GroupTypeId; y.Name = x.Name; }, cancellationToken);

		await Synchronize(personMasterdata.Statuses, database.Status, x => new(x.Id, x.Name), (x, y) => y.Name = x.Name, cancellationToken);

		await Task.WhenAll(
			SynchronizeGroups(cancellationToken),
			SynchronizePeople(cancellationToken)
		);

		await SynchronizeGroupMembers(cancellationToken);
	}

	private async Task SynchronizeGroups(CancellationToken cancellationToken)
	{
		await SynchronizePaginated(
			await churchTools.GetGroups(cancellationToken),
			database.Groups,
			x => new(x.Id, x.GroupTypeId, x.Name),
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
		await SynchronizePaginated(
			await churchTools.GetPeople(cancellationToken),
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

	private async ValueTask Synchronize<TSrc, TDest>(List<TSrc> apiResponses, DbSet<TDest> cachedSet, Func<TSrc, TDest> convert, Action<TSrc, TDest> update, CancellationToken cancellationToken)
		where TSrc : IIdentifiable<int>
		where TDest : class, IIdentifiable<int>
	{
		List<TDest> cachedValues = await cachedSet.OrderBy(x => x.Id).ToListAsync(cancellationToken);
		await Synchronize<TSrc, TDest, int>(apiResponses, cachedSet, cachedValues, convert, update, cancellationToken);
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

		await database.SaveChangesAsync(cancellationToken);
	}

	private async ValueTask SynchronizePaginated<TSrc, TDest>(List<TSrc> apiResponses, DbSet<TDest> cachedSet, Func<TSrc, TDest> convert, Action<TSrc, TDest> update, CancellationToken cancellationToken)
		where TSrc : IIdentifiable<int>
		where TDest : class, IIdentifiable<int>
	{
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<TDest> cachedValues = await cachedSet.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		foreach ((TSrc? response, TDest? entity) in apiResponses.ContrastWith<TSrc, TDest, int>(cachedValues))
		{
			if (response == null)
				// TODO: This is a paginated API. Make sure this item really doesn't exist anymore before deleting it
				cachedSet.Remove(entity!);
			else if (entity == null)
				cachedSet.Add(convert(response));
			else
				update(response, entity);
		}

		await database.SaveChangesAsync(cancellationToken);
	}
}
