using Korga.Server.Ldap.Internal;
using System;
using System.DirectoryServices.Protocols;

namespace Korga.Server.Ldap;

public class LdapMapper : IDisposable
{
    private readonly LdapConnection connection;

    public LdapMapper(LdapConnection connection)
    {
        this.connection = connection;
    }

    public void Add<T>(string distinguishedName, T entry) where T : notnull
    {
        ILdapSerializer<T> serializer = CreateSerializer<T>();
        var attributes = new AttributeCollection();
        serializer.Serialize(attributes, entry);
        var request = new AddRequest(distinguishedName, attributes.ToArray());
        var response = (AddResponse)connection.SendRequest(request);
    }

    public void Delete(string distinguishedName)
    {
        var request = new DeleteRequest(distinguishedName);
        var response = (DeleteResponse)connection.SendRequest(request);
    }

    public T[] Search<T>(string distinguishedName, string ldapFilter, SearchScope searchScope)
    {
        ILdapSerializer<T> serializer = CreateSerializer<T>();
        var request = new SearchRequest(distinguishedName, ldapFilter, searchScope, serializer.Attributes);
        var response = (SearchResponse)connection.SendRequest(request);

        var result = new T[response.Entries.Count];

        for (int i = 0; i < response.Entries.Count; i++)
        {
            var attributes = new AttributeCollection(response.Entries[i].Attributes);
            result[i] = serializer.Deserialize(attributes);
        }

        return result;
    }

    private ILdapSerializer<T> CreateSerializer<T>()
    {
        const string @namespace = nameof(Korga) + "." + nameof(Server) + "." + nameof(Ldap) + "." + nameof(Internal);

        Type serializerType = Type.GetType($"{@namespace}.{typeof(T).Name}Serializer")
            ?? throw new NotSupportedException($"Object class {typeof(T).FullName} is not supported");

        return (ILdapSerializer<T>)(Activator.CreateInstance(serializerType)
            ?? throw new NotSupportedException($"The serializer for {typeof(T).FullName} is missing a parameterless constructor"));
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}
