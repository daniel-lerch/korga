using Korga.Server.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public class JobQueue<TController> : BackgroundService where TController : IJobController
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<JobQueue<TController>> logger;
    private readonly AsyncAutoResetEvent @event;

    public JobQueue(IServiceProvider serviceProvider, ILogger<JobQueue<TController>> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        @event = new AsyncAutoResetEvent(false);
    }

    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMinutes(10);

    public void EnsureRunning()
    {
        @event.Set();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (await QueryAndExecute(stoppingToken))
                await @event.WaitAsync(RetryInterval, stoppingToken);
            else
                await Task.Delay(RetryInterval, stoppingToken);
        }
    }

    private async ValueTask<bool> QueryAndExecute(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        var jobController = ActivatorUtilities.CreateInstance<TController>(scope.ServiceProvider);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!await jobController.FetchAndExecute(cancellationToken)) return true;
            }
            catch (TransientFailureException)
            {
                return false;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogCritical(ex, "An unhandled exception occurred in a background job");
                return false;
            }
        }
        return true;
    }
}
