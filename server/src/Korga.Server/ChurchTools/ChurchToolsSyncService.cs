using Korga.Server.ChurchTools.Api;
using Korga.Server.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DbGroup = Korga.ChurchTools.Entities.Group;
using DbPerson = Korga.ChurchTools.Entities.Person;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncService
{
	private readonly DatabaseContext database;
	private readonly IChurchToolsApiService churchTools;

	public ChurchToolsSyncService(DatabaseContext database, IChurchToolsApiService churchTools)
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
			SynchronizeGroups(cancellationToken).AsTask(),
			SynchronizePeople(cancellationToken).AsTask()
		);
	}

	private async ValueTask Synchronize<TSrc, TDest>(List<TSrc> apiResponses, DbSet<TDest> cachedSet, Func<TSrc, TDest> convert, Action<TSrc, TDest> update, CancellationToken cancellationToken)
		where TSrc : IIdentifiable<int>
		where TDest : class, IIdentifiable<int>
	{
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<TDest> cachedValues = await cachedSet.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		foreach ((TSrc? response, TDest? entity) in apiResponses.ContrastWith<TSrc, TDest, int>(cachedValues))
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

	private async ValueTask SynchronizeGroups(CancellationToken cancellationToken)
	{
		List<Group> apiResponses = await churchTools.GetGroups(cancellationToken);
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<DbGroup> cachedValues = await database.Groups.ToListAsync(cancellationToken);

		foreach ((Group? response, DbGroup? entity) in apiResponses.ContrastWith<Group, DbGroup, int>(cachedValues))
		{
			if (response == null)
				// TODO: This is a paginated API. Make sure this item really doesn't exist anymore before deleting it
				database.Groups.Remove(entity!);
			else if (entity == null)
				database.Groups.Add(new(response.Id, response.Information["groupTypeId"].GetInt32(), response.Name));
			else
			{
				entity.GroupTypeId = response.Information["groupTypeId"].GetInt32();
				entity.Name = response.Name;
			}
		}

		await database.SaveChangesAsync(cancellationToken);
	}

	private async ValueTask SynchronizePeople(CancellationToken cancellationToken)
	{
		List<Person> apiResponses = await churchTools.GetPeople(cancellationToken);
		apiResponses.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<DbPerson> cachedValues = await database.People.ToListAsync(cancellationToken);

		foreach ((Person? response, DbPerson? entity) in apiResponses.ContrastWith<Person, DbPerson, int>(cachedValues))
		{
			if (response == null)
				// TODO: This is a paginated API. Make sure this item really doesn't exist anymore before deleting it
				database.People.Remove(entity!);
			else if (entity == null)
				database.People.Add(new(response.Id, response.StatusId, response.FirstName, response.LastName, response.Email));
			else
			{
				entity.StatusId = response.StatusId;
				entity.FirstName = response.FirstName;
				entity.LastName = response.LastName;
				entity.Email = response.Email;
			}
		}

		await database.SaveChangesAsync(cancellationToken);
	}
}
