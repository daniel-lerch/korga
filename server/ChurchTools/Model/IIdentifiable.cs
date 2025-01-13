using System;

namespace ChurchTools.Model;

public interface IIdentifiable<TKey> where TKey : IComparable<TKey>
{
    TKey Id { get; }
}
