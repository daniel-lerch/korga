using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public abstract class RepeatedExecutionService : BackgroundService
{
    private readonly TimeSpan interval;

    protected RepeatedExecutionService(TimeSpan interval)
    {
        this.interval = interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stopwatch stopwatch = new();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                stopwatch.Start();

                await ExecuteAsync(stoppingToken);

                stopwatch.Stop();
                TimeSpan timeout = interval - stopwatch.Elapsed;
                stopwatch.Reset();

                if (timeout.Ticks > 0) await Task.Delay(timeout, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    protected abstract ValueTask ExecuteOnce(CancellationToken stoppingToken);
}
