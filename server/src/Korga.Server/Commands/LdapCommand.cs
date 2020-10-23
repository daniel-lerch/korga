using Korga.Server.Configuration;
using Korga.Server.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0051 // Remove unused private members

namespace Korga.Server.Commands
{
    [Command("ldap")]
    [Subcommand(typeof(Create))]
    public class LdapCommand
    {
        private int OnExecute(CommandLineApplication app)
        {
            app.ShowHint();
            return 1;
        }

        [Command("create")]
        public class Create
        {
            private void OnExecute(IOptions<LdapOptions> options, LdapService ldap)
            {
                ldap.Add(options.Value.BaseDn, "organizationalUnit");
            }
        }
    }
}
