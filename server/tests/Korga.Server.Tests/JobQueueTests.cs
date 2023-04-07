using Korga.Server.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests;

public class JobQueueTests : IDisposable
{
    private const string transientFailure = "TRANSIENT FAILURE";

    private readonly JobStorage<string> storage;
    private readonly ServiceProvider serviceProvider;
    private readonly JobQueue<StringJobController> jobQueue;

    public JobQueueTests()
    {
        storage = new();
        ServiceCollection services = new();
        services.AddSingleton(storage);
        serviceProvider = services.BuildServiceProvider();
        jobQueue = new(serviceProvider, NullLogger<JobQueue<StringJobController>>.Instance);
    }

    public void Dispose()
    {
        serviceProvider.Dispose();
    }

    [Fact]
    public async Task TestRunPendingOnStart()
    {
        storage.Data.Enqueue("Hello");
        storage.Data.Enqueue("World");
        await jobQueue.StartAsync(CancellationToken.None);
        await jobQueue.StopAsync(CancellationToken.None);
        Assert.Equal(2, storage.Executed);
    }

    [Fact]
    public async Task TestRunImmediately()
    {
        await jobQueue.StartAsync(CancellationToken.None);
        storage.Data.Enqueue("Hello");
        storage.Data.Enqueue("World");
        jobQueue.EnsureRunning();
        await Task.Delay(50);
        await jobQueue.StopAsync(CancellationToken.None);
        Assert.Equal(2, storage.Executed);
    }

    [Fact]
    public async Task TestRetry()
    {
        jobQueue.RetryInterval = TimeSpan.FromMilliseconds(200);
        await jobQueue.StartAsync(CancellationToken.None);
        storage.Data.Enqueue(transientFailure);
        storage.Data.Enqueue("Hello");
        storage.Data.Enqueue("World");
        jobQueue.EnsureRunning();
        await Task.Delay(100);
        Assert.Equal(0, storage.Executed);
        await Task.Delay(200);
        Assert.Equal(2, storage.Executed);
    }

    [Fact]
    public async Task TestPolling()
    {
        jobQueue.RetryInterval = TimeSpan.FromMilliseconds(200);
        await jobQueue.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        storage.Data.Enqueue("Hello");
        storage.Data.Enqueue("World");
        await Task.Delay(200);
        Assert.Equal(2, storage.Executed);
    }


    private class JobStorage<T>
    {
        public Queue<T> Data { get; } = new();
        public int Executed { get; set; }
    }

    private class StringJobController : OneAtATimeJobController<string>
    {
        private readonly JobStorage<string> storage;

        public StringJobController(JobStorage<string> storage)
        {
            this.storage = storage;
        }

        protected override ValueTask ExecuteJob(string data, CancellationToken cancellationToken)
        {
            if (data == transientFailure)
                return ValueTask.FromException(new TransientFailureException(transientFailure));
            storage.Executed++;
            return ValueTask.CompletedTask;
        }

        protected override ValueTask<string?> NextPendingOrDefault(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(storage.Data.TryDequeue(out string? data) ? data : null);
        }
    }
}
