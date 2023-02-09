using System;
using System.Collections.Generic;

namespace Korga.Server.Extensions;

public static class CollectionExtensions
{
	public static IEnumerable<(TSrc?, TDest?)> ContrastWith<TSrc, TDest, TKey>(this IEnumerable<TSrc> source, IEnumerable<TDest> destination)
	where TSrc : IIdentifiable<TKey>
	where TDest : IIdentifiable<TKey>
	where TKey : IComparable<TKey>
	{
		IEnumerator<TSrc> sourceEnumerator = source.GetEnumerator();
		IEnumerator<TDest> destinationEnumerator = destination.GetEnumerator();
		if (!sourceEnumerator.MoveNext())
		{
			// Remove all entities
			while (destinationEnumerator.MoveNext()) yield return (default, destinationEnumerator.Current);
			yield break;
		}
		if (!destinationEnumerator.MoveNext())
		{
			// Add item retrieved before when checking whether source is empty
			yield return (sourceEnumerator.Current, default);

			// Add all items
			while (sourceEnumerator.MoveNext()) yield return (sourceEnumerator.Current, default);
			yield break;
		}

		while (true)
		{
			TKey itemId = sourceEnumerator.Current.Id;
			TKey entityId = destinationEnumerator.Current.Id;
			if (itemId.CompareTo(entityId) < 0)
			{
				// Add item
				yield return (sourceEnumerator.Current, default);

				if (!sourceEnumerator.MoveNext())
				{
					// Last item reached -> Remove all following entities
					while (destinationEnumerator.MoveNext()) yield return (default, destinationEnumerator.Current);
					break;
				}
			}
			else if (itemId.CompareTo(entityId) > 0)
			{
				// Remove entity
				yield return (default, destinationEnumerator.Current);

				if (!destinationEnumerator.MoveNext())
				{
					// Last entity reached -> Add all following items
					while (sourceEnumerator.MoveNext()) yield return (sourceEnumerator.Current, default);
					break;
				}
			}
			else
			{
				// Update item
				yield return (sourceEnumerator.Current, destinationEnumerator.Current);

				if (!sourceEnumerator.MoveNext())
				{
					// Last item reached -> Remove all following entities
					while (destinationEnumerator.MoveNext()) yield return (default, destinationEnumerator.Current);
					break;
				}
				if (!destinationEnumerator.MoveNext())
				{
					// Add item retrieved before when checking whether source is depleted
					yield return (sourceEnumerator.Current, default);

					// Last entity reached -> Add all following items
					while (sourceEnumerator.MoveNext()) yield return (sourceEnumerator.Current, default);
					break;
				}
			}
		}
	}
}
