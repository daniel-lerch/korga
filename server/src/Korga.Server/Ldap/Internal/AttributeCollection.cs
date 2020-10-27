using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Korga.Server.Ldap.Internal
{
    internal class AttributeCollection
    {
        private readonly Dictionary<string, DirectoryAttribute> attributes;

        public AttributeCollection()
        {
            attributes = new Dictionary<string, DirectoryAttribute>(StringComparer.OrdinalIgnoreCase);
        }

        public AttributeCollection(SearchResultAttributeCollection searchResults)
            : this()
        {
            IDictionaryEnumerator enumerator = searchResults.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = (string)enumerator.Key;
                var value = (DirectoryAttribute)enumerator.Value!;
                attributes.Add(key, value);
            }
        }

        public void Add(string name, string value)
        {
            attributes.Add(name, new DirectoryAttribute(name, value));
        }

        public void AddIfNotEmpty(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                attributes.Add(name, new DirectoryAttribute(name, value));
        }

        public string GetRequiredValue(string name)
        {
            var attribute = attributes[name];
            if (attribute.Count == 0)
                throw new InvalidOperationException();
            else
                return (string)attribute[0];
        }

        public string? GetValue(string name)
        {
            attributes.TryGetValue(name, out var attribute);
            return attribute?.Count > 0 ? attribute[0] as string : null;
        }

        public DirectoryAttribute[] ToArray()
        {
            var values = attributes.Values;
            var result = new DirectoryAttribute[values.Count];
            values.CopyTo(result, 0);
            return result;
        }
    }
}
