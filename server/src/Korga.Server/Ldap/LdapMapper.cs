using Korga.Server.Ldap.Internal;
using System;
using System.DirectoryServices.Protocols;

namespace Korga.Server.Ldap
{
    public class LdapMapper
    {
        private readonly LdapConnection connection;

        public LdapMapper(LdapConnection connection)
        {
            this.connection = connection;
        }

        private void Add<T>(string distinguishedName, T entry) where T : notnull
        {
            const string @namespace = nameof(Korga) + "." + nameof(Server) + "." + nameof(Ldap) + "." + nameof(Internal);

            Type serializerType = Type.GetType($"{@namespace}.{entry.GetType().Name}Serializer")
                ?? throw new NotSupportedException($"Object class {entry.GetType().FullName} is not supported");

            var serializer = (ILdapSerializer<T>)(Activator.CreateInstance(serializerType)
                ?? throw new NotSupportedException($"The serializer for {entry.GetType().FullName} is missing a parameterless constructor"));

            var attributes = new AttributeCollection();
            serializer.Serialize(attributes, entry);
            var request = new AddRequest(distinguishedName, attributes.ToArray());
            var response = (AddResponse)connection.SendRequest(request);
        }
    }
}
