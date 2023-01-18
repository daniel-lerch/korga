using System.Collections.Generic;
using System;

namespace Korga.Server.Utilities;

public abstract class CollectionSynchronizer<TSrc, TDest, TKey> where TKey : IComparable<TKey>
{
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
