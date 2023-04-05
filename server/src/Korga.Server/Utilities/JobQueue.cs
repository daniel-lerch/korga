using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public class JobQueue<TController, TData> : BackgroundService where TController : IJobController<TData>
{
    private readonly IServiceProvider serviceProvider;
    private readonly AsyncAutoResetEvent @event;

    public JobQueue(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        @event = new AsyncAutoResetEvent(false);
    }

    public void EnsureRunning()
    {
        @event.Set();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await QueryAndExecute(stoppingToken);
            await @event.WaitAsync(stoppingToken);
        }
    }

    private async ValueTask QueryAndExecute(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        var jobController = ActivatorUtilities.CreateInstance<TController>(scope.ServiceProvider);
        while (!cancellationToken.IsCancellationRequested)
        {
            var pending = await jobController.NextPendingOrDefault(cancellationToken);
            if (pending == null || await jobController.ExecuteJob(pending, cancellationToken))
                break;
        }
    }
}
