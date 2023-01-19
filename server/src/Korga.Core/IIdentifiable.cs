using System;

namespace Korga;

public interface IIdentifiable<TKey> where TKey : IComparable<TKey>
{
	TKey Id { get; }
}
