using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Korga.Server.Ldap.Internal
{
    internal class AttributeCollection
    {
        private readonly List<DirectoryAttribute> attributes;

        public AttributeCollection()
        {
            attributes = new List<DirectoryAttribute>();
        }

        public void Add(string name, string value)
        {
            attributes.Add(new DirectoryAttribute(name, value));
        }

        public void AddIfNotEmpty(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                attributes.Add(new DirectoryAttribute(name, value));
        }

        public DirectoryAttribute[] ToArray() => attributes.ToArray();
    }
}
