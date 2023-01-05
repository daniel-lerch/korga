using Korga.ChurchTools.Entities;
using Korga.Server.ChurchTools.Api;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.ChurchTools;

public class ChurchToolsSyncService
{
	private readonly ChurchToolsApiService churchTools;
	private readonly DatabaseContext database;

	public ChurchToolsSyncService(ChurchToolsApiService churchTools, DatabaseContext database)
	{
		this.churchTools = churchTools;
		this.database = database;
	}

	public async ValueTask Sync(CancellationToken cancellationToken)
	{
		PersonMasterdata personMasterdata = await churchTools.GetPersonMasterdata(cancellationToken);
		await SyncGroupTypes(personMasterdata.GroupTypes, cancellationToken);
	}

	private async ValueTask SyncGroupTypes(List<PersonMasterdata.GroupType> groupTypes, CancellationToken cancellationToken)
	{
		groupTypes.Sort((x, y) => x.Id.CompareTo(y.Id));
		List<GroupType> dbGroupTypes = await database.GroupTypes.OrderBy(x => x.Id).ToListAsync(cancellationToken);

		Sync(groupTypes, x => x.Id, dbGroupTypes, x => x.Id, x => database.GroupTypes.Add(new GroupType(x.Id, x.Name)), (x, y) => throw new NotImplementedException(), x => database.GroupTypes.Remove(x));
	}

	private void Sync<TItem, TEntity>(IReadOnlyList<TItem> items, Func<TItem, int> getItemId, IReadOnlyList<TEntity> entities, Func<TEntity, int> getEntityId,
		Action<TItem> add, Action<TItem, TEntity> update, Action<TEntity> delete)
	{
		if (items.Count == 0)
		{
			// Remove all entities
			foreach (TEntity entity in entities) delete(entity);
		}
		else if (entities.Count == 0)
		{
			// Add all items
			foreach (TItem item in items) add(item);
		}
		else
		{
			for (int itemIdx = 0, entityIdx = 0; ;)
			{
				int itemId = getItemId(items[itemIdx]);
				int entityId = getEntityId(entities[entityIdx]);
				if (itemId < entityId)
				{
					// Add item
					add(items[itemIdx]);

					itemIdx++;
					if (itemIdx >= items.Count)
					{
						// Last item reached -> Remove all following entities
						for (int i = entityIdx + 1; i < entities.Count; i++) delete(entities[i]);
						break;
					}
				}
				else if (itemId > entityId)
				{
					// Remove entity
					delete(entities[entityIdx]);

					entityIdx++;
					if (entityIdx >= entities.Count)
					{
						// Last entity reached -> Add all following items
						for (int i = itemIdx + 1; i < items.Count; i++) add(items[i]);
						break;
					}
				}
				else
				{
					// Update item
					update(items[itemIdx], entities[entityIdx]);

					itemIdx++;
					entityIdx++;
					if (itemIdx >= items.Count)
					{
						// Last item reached -> Remove all following entities
						for (int i = entityIdx + 1; i < entities.Count; i++) delete(entities[i]);
						break;
					}
					else if (entityIdx >= entities.Count)
					{
						// Last entity reached -> Add all following items
						for (int i = itemIdx + 1; i < items.Count; i++) add(items[i]);
						break;
					}
				}
			}
		}
	}
}
