using System.Collections.Generic;
using System;

namespace Korga.Server.Utilities;

public abstract class CollectionSynchronizer<TSrc, TDest, TKey> where TKey : IComparable<TKey>
{
	public void Sync(IReadOnlyList<TSrc> items, IReadOnlyList<TDest> entities)
	{
		if (items.Count == 0)
		{
			// Remove all entities
			foreach (TDest entity in entities) Remove(entity);
		}
		else if (entities.Count == 0)
		{
			// Add all items
			foreach (TSrc item in items) Add(item);
		}
		else
		{
			for (int itemIdx = 0, entityIdx = 0; ;)
			{
				TKey itemId = GetSrcKey(items[itemIdx]);
				TKey entityId = GetDstKey(entities[entityIdx]);
				if (itemId.CompareTo(entityId) < 0)
				{
					// Add item
					Add(items[itemIdx]);

					itemIdx++;
					if (itemIdx >= items.Count)
					{
						// Last item reached -> Remove all following entities
						for (int i = entityIdx + 1; i < entities.Count; i++) Remove(entities[i]);
						break;
					}
				}
				else if (itemId.CompareTo(entityId) > 0)
				{
					// Remove entity
					Remove(entities[entityIdx]);

					entityIdx++;
					if (entityIdx >= entities.Count)
					{
						// Last entity reached -> Add all following items
						for (int i = itemIdx + 1; i < items.Count; i++) Add(items[i]);
						break;
					}
				}
				else
				{
					// Update item
					Update(items[itemIdx], entities[entityIdx]);

					itemIdx++;
					entityIdx++;
					if (itemIdx >= items.Count)
					{
						// Last item reached -> Remove all following entities
						for (int i = entityIdx + 1; i < entities.Count; i++) Remove(entities[i]);
						break;
					}
					else if (entityIdx >= entities.Count)
					{
						// Last entity reached -> Add all following items
						for (int i = itemIdx + 1; i < items.Count; i++) Add(items[i]);
						break;
					}
				}
			}
		}
	}


	public void Sync(IReadOnlyCollection<TSrc> source, IReadOnlyCollection<TDest> destination)
	{
		if (source.Count == 0)
		{
			// Remove all entities
			foreach (TDest dest in destination) Remove(dest);
		}
		else if (destination.Count == 0)
		{
			// Add all items
			foreach (TSrc src in source) Add(src);
		}
		else
		{
			IEnumerator<TSrc> sourceEnumerator = source.GetEnumerator();
			sourceEnumerator.MoveNext();
			IEnumerator<TDest> destinationEnumerator = destination.GetEnumerator();
			destinationEnumerator.MoveNext();

			while (true)
			{
				TKey itemId = GetSrcKey(sourceEnumerator.Current);
				TKey entityId = GetDstKey(destinationEnumerator.Current);
				if (itemId.CompareTo(entityId) < 0)
				{
					// Add item
					Add(sourceEnumerator.Current);

					if (!sourceEnumerator.MoveNext())
					{
						// Last item reached -> Remove all following entities
						while (destinationEnumerator.MoveNext()) Remove(destinationEnumerator.Current);
						break;
					}
				}
				else if (itemId.CompareTo(entityId) > 0)
				{
					// Remove entity
					Remove(destinationEnumerator.Current);

					if (!destinationEnumerator.MoveNext())
					{
						// Last entity reached -> Add all following items
						while (sourceEnumerator.MoveNext()) Add(sourceEnumerator.Current);
						break;
					}
				}
				else
				{
					// Update item
					Update(sourceEnumerator.Current, destinationEnumerator.Current);

					if (!sourceEnumerator.MoveNext())
					{
						// Last item reached -> Remove all following entities
						while (destinationEnumerator.MoveNext()) Remove(destinationEnumerator.Current);
						break;
					}
					if (!destinationEnumerator.MoveNext())
					{
						// Last entity reached -> Add all following items
						while (sourceEnumerator.MoveNext()) Add(sourceEnumerator.Current);
						break;
					}
				}
			}
		}
	}

	protected abstract TKey GetSrcKey(TSrc src);
	protected abstract TKey GetDstKey(TDest dest);

	protected abstract void Add(TSrc src);
	protected abstract void Update(TSrc src, TDest dest);
	protected abstract void Remove(TDest dest);
}
