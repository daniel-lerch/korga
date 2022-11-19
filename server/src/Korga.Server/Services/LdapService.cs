using Korga.Server.Configuration;
using Korga.Server.Ldap;
using Korga.Server.Ldap.ObjectClasses;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;

namespace Korga.Server.Services;

public class LdapService : IDisposable
{
    private readonly IOptions<LdapOptions> options;
    private readonly LdapConnection connection;
    private readonly LdapMapper mapper;
    private bool disposed;

    // LdapConnection provides only one APM async method which interally uses polling.
    // In most cases the synchornous methods will be faster and more efficient.

    public LdapService(IOptions<LdapOptions> options)
    {
        this.options = options;

        connection = new LdapConnection(options.Value.Host);
        connection.AuthType = AuthType.Basic;
        connection.SessionOptions.ProtocolVersion = 3;
        connection.Bind(new NetworkCredential(options.Value.BindDn, options.Value.BindPassword));

        mapper = new LdapMapper(connection);
    }

    public InetOrgPerson[] GetMembers()
    {
        if (disposed) throw new ObjectDisposedException(nameof(LdapService));

        return mapper.Search<InetOrgPerson>(options.Value.BaseDn, "(objectClass=inetOrgPerson)", SearchScope.OneLevel);
    }

    public InetOrgPerson? GetMember(string uid)
    {
        if (disposed) throw new ObjectDisposedException(nameof(LdapService));

        return mapper.Search<InetOrgPerson>(options.Value.BaseDn, $"(& (objectClass=inetOrgPerson) (uid={uid}))", SearchScope.OneLevel).SingleOrDefault();
    }

    public void AddOrganizationalUnit(string distinguishedName, string name)
    {
        if (disposed) throw new ObjectDisposedException(nameof(LdapService));

        mapper.Add(distinguishedName, new OrganizationalUnit(name));
    }

    public void AddPerson(string uid, string givenName, string familyName, string mailAddress)
    {
        if (disposed) throw new ObjectDisposedException(nameof(LdapService));

        string name = $"{givenName} {familyName}";

        var person = new InetOrgPerson(cn: name, familyName)
        {
            GivenName = givenName,
            DisplayName = name,
            Mail = mailAddress
        };

        mapper.Add($"uid={uid},{options.Value.BaseDn}", person);
    }

    public void SavePerson(string uid, InetOrgPerson person)
    {
        mapper.SaveChanges($"uid={uid},{options.Value.BaseDn}", person);
    }

    public void Delete(string distinguishedName)
    {
        if (disposed) throw new ObjectDisposedException(nameof(LdapService));

        mapper.Delete(distinguishedName);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            connection.Dispose();
        }

        disposed = true;
    }
}
