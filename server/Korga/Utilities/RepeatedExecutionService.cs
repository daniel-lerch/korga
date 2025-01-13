using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Utilities;

public abstract class RepeatedExecutionService : BackgroundService
{
    protected readonly ILogger logger;

    protected RepeatedExecutionService(ILogger logger)
    {
        this.logger = logger;
    }

    protected TimeSpan Interval { get; set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stopwatch stopwatch = new();

        while (!stoppingToken.IsCancellationRequested)
        {
			stopwatch.Start();
			
            try
			{
                await ExecuteOnce(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An unhandled exception occurred in a background service");
            }

			stopwatch.Stop();
			TimeSpan timeout = Interval - stopwatch.Elapsed;
			stopwatch.Reset();

			if (timeout.Ticks > 0) await Task.Delay(timeout, stoppingToken);
		}
	}

    protected abstract ValueTask ExecuteOnce(CancellationToken stoppingToken);
}
