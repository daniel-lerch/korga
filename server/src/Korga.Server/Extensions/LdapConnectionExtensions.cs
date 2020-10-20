using System.DirectoryServices.Protocols;
using System.Threading.Tasks;

namespace Korga.Server.Extensions
{
    public static class LdapConnectionExtensions
    {
        public static Task<DirectoryResponse> SendRequestAsync(this LdapConnection connection, DirectoryRequest request)
        {
            return Task<DirectoryResponse>.Factory.FromAsync(
                connection.BeginSendRequest,
                connection.EndSendRequest,
                request,
                PartialResultProcessing.NoPartialResultSupport,
                state: null);
        }
    }
}
