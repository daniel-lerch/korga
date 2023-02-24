namespace Korga.Extensions;

public static class CollectionExtensions
{
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
	{
		foreach (var item in values)
		{
			collection.Add(item);
		}
	}
}
