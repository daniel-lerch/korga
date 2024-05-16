using Korga.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.EmailRelay;

public class EmailRelayHostedService : RepeatedExecutionService
{
    private readonly IServiceProvider serviceProvider;

    public EmailRelayHostedService(IOptions<EmailRelayOptions> options, ILogger<EmailRelayHostedService> logger, IServiceProvider serviceProvider)
        : base(logger)
    {
        this.serviceProvider = serviceProvider;

        Interval = TimeSpan.FromMinutes(options.Value.RetrievalIntervalInMinutes);
    }

    protected override async ValueTask ExecuteOnce(CancellationToken stoppingToken)
    {
        using IServiceScope serviceScope = serviceProvider.CreateScope();
        ImapReceiverService imapService = serviceScope.ServiceProvider.GetRequiredService<ImapReceiverService>();

        await imapService.FetchAsync(stoppingToken);
    }
}
