using Korga.Server.Configuration;
using Korga.Server.Ldap;
using Korga.Server.Ldap.ObjectClasses;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Korga.Server.Services
{
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

        public int GetMembers()
        {
            if (disposed) throw new ObjectDisposedException(nameof(LdapService));

            var request = new SearchRequest(options.Value.BaseDn, "(objectClass=inetOrgPerson)", SearchScope.OneLevel);
            var response = (SearchResponse)connection.SendRequest(request);
            return response.Entries.Count;
        }

        public void AddOrganizationalUnit(string distinguishedName, string name)
        {
            if (disposed) throw new ObjectDisposedException(nameof(LdapService));

            mapper.Add(distinguishedName, new OrganizationalUnit(name));
        }

        public void AddPerson(string distinguishedName, string givenName, string familyName)
        {
            if (disposed) throw new ObjectDisposedException(nameof(LdapService));

            string name = $"{givenName} {familyName}";

            var person = new InetOrgPerson(cn: name, familyName)
            {
                GivenName = givenName,
                DisplayName = name
            };

            mapper.Add(distinguishedName, person);
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
}
