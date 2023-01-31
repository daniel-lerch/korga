using Korga.Server.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Korga.Server.Tests;

public class CollectionExtensionsTests
{
	[Fact]
	public void TestClear()
	{
		List<Source> source = new();
		List<Destination> destination = new() { new() { Id = 5 }, new() { Id = 7 } };

		var expected = new (Source?, Destination?)[] { (null, destination[0]), (null, destination[1]) };

		Assert.Equal(expected, source.ContrastWith<Source, Destination, int>(destination).ToArray());
	}

	[Fact]
	public void TestInit()
	{
		List<Source> source = new() { new() { Id = 3 }, new() { Id = 4 } };
		List<Destination> destination = new();

		var expected = new (Source?, Destination?)[] { (source[0], null), (source[1], null) };

		Assert.Equal(expected, source.ContrastWith<Source, Destination, int>(destination).ToArray());
	}

	[Fact]
	public void TestUpdate()
	{
		List<Source> source = new() { new() { Id = 1 }, new() { Id = 3, SortKey = 5 } };
		List<Destination> destination = new() { new() { Id = 1 }, new() { Id = 3 } };

		var expected = new (Source?, Destination?)[] { (source[0], destination[0]), (source[1], destination[1]) };

		Assert.Equal(expected, source.ContrastWith<Source, Destination, int>(destination).ToArray());
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
}
