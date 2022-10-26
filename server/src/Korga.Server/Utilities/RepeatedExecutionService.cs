﻿using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Korga.Server.Utilities;

public abstract class RepeatedExecutionService : BackgroundService
{
    protected TimeSpan Interval { get; set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stopwatch stopwatch = new();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                stopwatch.Start();

                await ExecuteOnce(stoppingToken);

                stopwatch.Stop();
                TimeSpan timeout = Interval - stopwatch.Elapsed;
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
