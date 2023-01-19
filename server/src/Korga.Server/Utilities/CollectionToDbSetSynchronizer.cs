using Microsoft.EntityFrameworkCore;
using System;

namespace Korga.Server.Utilities
{
	public abstract class CollectionToDbSetSynchronizer<TSrc, TDest, TKey> : CollectionSynchronizer<TSrc, TDest, TKey>
		where TSrc : IIdentifiable<TKey>
		where TDest : class, IIdentifiable<TKey>
		where TKey : IComparable<TKey>
	{
		private readonly DbSet<TDest> destinationSet;

		protected CollectionToDbSetSynchronizer(DbSet<TDest> destinationSet)
		{
			this.destinationSet = destinationSet;
		}

		protected override void Add(TSrc src)
		{
			destinationSet.Add(Convert(src));
		}

		protected override void Remove(TDest dest)
		{
			destinationSet.Remove(dest);
		}

		protected abstract TDest Convert(TSrc src);
	}
}
