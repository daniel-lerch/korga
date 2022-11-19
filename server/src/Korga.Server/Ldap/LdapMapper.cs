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

    public void Add<T>(string distinguishedName, T entry) where T : IObjectClass<T>
    {
        AttributeCollection attributes = new();
        T.Serialize(attributes, entry);
        var request = new AddRequest(distinguishedName, attributes.ToArray());
        var response = (AddResponse)connection.SendRequest(request);
    }

    public void Delete(string distinguishedName)
    {
        var request = new DeleteRequest(distinguishedName);
        var response = (DeleteResponse)connection.SendRequest(request);
    }

    public T[] Search<T>(string distinguishedName, string ldapFilter, SearchScope searchScope) where T : IObjectClass<T>
    {
        var request = new SearchRequest(distinguishedName, ldapFilter, searchScope, T.Attributes);
        var response = (SearchResponse)connection.SendRequest(request);

        var result = new T[response.Entries.Count];

        for (int i = 0; i < response.Entries.Count; i++)
        {
            var attributes = new AttributeCollection(response.Entries[i].Attributes);
            result[i] = T.Deserialize(attributes);
        }

        return result;
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}
