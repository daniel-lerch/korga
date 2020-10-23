using Korga.Server.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Korga.Server.Services
{
    public class LdapService : IDisposable
    {
        private readonly IOptions<LdapOptions> options;
        private readonly Lazy<LdapConnection> connection;
        private bool disposed;

        // LdapConnection provides only one APM async method which interally uses polling.
        // In most cases the synchornous methods will be faster and more efficient.

        public LdapService(IOptions<LdapOptions> options)
        {
            this.options = options;

            connection = new Lazy<LdapConnection>(() =>
            {
                var connection = new LdapConnection(options.Value.Host);
                connection.AuthType = AuthType.Basic;
                connection.SessionOptions.ProtocolVersion = 3;
                connection.Bind(new NetworkCredential(options.Value.BindDn, options.Value.BindPassword));
                return connection;
            }, isThreadSafe: true);
        }

        public int GetMembers()
        {
            if (disposed) throw new ObjectDisposedException(nameof(LdapService));

            var request = new SearchRequest(options.Value.BaseDn, ldapFilter: null, SearchScope.OneLevel);
            var response = (SearchResponse)connection.Value.SendRequest(request);
            return response.Entries.Count;
        }

        public void Add(string distinguishedName, string objectClass)
        {
            if (disposed) throw new ObjectDisposedException(nameof(LdapService));

            var request = new AddRequest(distinguishedName, objectClass);
            var response = (AddResponse)connection.Value.SendRequest(request);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (connection.IsValueCreated) connection.Value.Dispose();
            }

            disposed = true;
        }
    }
}
