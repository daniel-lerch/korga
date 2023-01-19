using Korga.Server.Utilities;
using System.Collections.Generic;
using Xunit;

namespace Korga.Server.Tests;

public class CollectionSynchronizerTests
{
	[Fact]
	public void TestClear()
	{
		List<Source> source = new();
		List<Destination> destination = new() { new() { Id = 5 }, new() { Id = 7 } };

		ListSynchronizer synchronizer = new();
		synchronizer.Sync(source, destination);
		foreach (Destination added in synchronizer.Added) destination.Add(added);
		foreach (Destination removed in synchronizer.Removed) destination.Remove(removed);

		Assert.Empty(destination);
	}

	[Fact]
	public void TestInit()
	{
		List<Source> source = new() { new() { Id = 3 }, new() { Id = 4 } };
		List<Destination> destination = new();

		ListSynchronizer synchronizer = new();
		synchronizer.Sync(source, destination);
		foreach (Destination added in synchronizer.Added) destination.Add(added);
		foreach (Destination removed in synchronizer.Removed) destination.Remove(removed);

		Assert.Equal(2, destination.Count);
		Assert.Equal(3, destination[0].Id);
		Assert.Equal(4, destination[1].Id);
	}

	[Fact]
	public void TestUpdate()
	{
		List<Source> source = new() { new() { Id = 1 }, new() { Id = 3, SortKey = 5 } };
		List<Destination> destination = new() { new() { Id = 1 }, new() { Id = 3} };

		ListSynchronizer synchronizer = new();
		synchronizer.Sync(source, destination);

		Assert.Empty(synchronizer.Added);
		Assert.Empty(synchronizer.Removed);

		Assert.Equal(2, destination.Count);
		Assert.Equal(0, destination[0].SortKey);
		Assert.Equal(5, destination[1].SortKey);
	}

	private class Source : IIdentifiable<int>
	{
		public int Id { get; set; }
		public int SortKey { get; set; }
	}

	private class Destination : IIdentifiable<int>
	{
		public int Id { get; set; }
		public int SortKey { get; set; }
	}

	private class ListSynchronizer : CollectionSynchronizer<Source, Destination, int>
	{
		public List<Destination> Added { get; } = new();

		public List<Destination> Removed { get; } = new();

		protected override void Add(Source src)
		{
			Added.Add(new() { Id = src.Id, SortKey = src.SortKey });
		}

		protected override void Remove(Destination dest)
		{
			Removed.Add(dest);
		}

		protected override void Update(Source src, Destination dest)
		{
			dest.SortKey = src.SortKey;
		}
	}
}
