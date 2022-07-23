using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.EmailRelay;

public class EmailRelayHostedService : IHostedService
{
    private readonly IOptions<EmailRelayOptions> options;
    private readonly ImapClient imap;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options)
    {
        this.options = options;
        imap = new();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await imap.ConnectAsync(options.Value.ImapHost, options.Value.ImapPort, options.Value.ImapUseSsl);
        await imap.AuthenticateAsync(options.Value.ImapUsername, options.Value.ImapPassword);
        IMailFolder folder = await imap.GetFolderAsync("INBOX");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await imap.DisconnectAsync(quit: true);
    }

    public void Dispose()
    {
        imap.Dispose();
    }
}
