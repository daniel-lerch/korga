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
    private readonly JobStorage<Job> storage;
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
        storage.Data.Enqueue(new());
        storage.Data.Enqueue(new());
        await jobQueue.StartAsync(CancellationToken.None);
        await jobQueue.StopAsync(CancellationToken.None);
        Assert.Equal(2, storage.Executed);
    }

    [Fact]
    public async Task TestRunImmediately()
    {
        Job job1 = new();
        Job job2 = new();
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        await jobQueue.StartAsync(CancellationToken.None);
        storage.Data.Enqueue(job1);
        storage.Data.Enqueue(job2);
        jobQueue.EnsureRunning();
        await job1.CompletionSource.Task.WaitAsync(cts.Token);
        await job2.CompletionSource.Task.WaitAsync(cts.Token);
        await jobQueue.StopAsync(CancellationToken.None);
        Assert.Equal(2, storage.Executed);
    }

    [Fact]
    public async Task TestRetry()
    {
        Job job1 = new(transientFailure: true);
        Job job2 = new();
        Job job3 = new();
        jobQueue.RetryInterval = TimeSpan.FromMilliseconds(200);
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
        
        await jobQueue.StartAsync(CancellationToken.None);
        storage.Data.Enqueue(job1);
        storage.Data.Enqueue(job2);
        storage.Data.Enqueue(job3);
        jobQueue.EnsureRunning();
        await job1.CompletionSource.Task.WaitAsync(cts.Token);

        // This assert uses a timing constraint.
        // If this method is not scheduled within the retry interval,
        // the other jobs could be executed before this assert.
        Assert.Equal(0, storage.Executed);

        await job2.CompletionSource.Task.WaitAsync(cts.Token);
        await job3.CompletionSource.Task.WaitAsync(cts.Token);
        Assert.Equal(2, storage.Executed);
    }

    [Fact]
    public async Task TestPolling()
    {
        Job job1 = new();
        Job job2 = new();
        jobQueue.RetryInterval = TimeSpan.FromMilliseconds(200);
        CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));

        await jobQueue.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        storage.Data.Enqueue(job1);
        storage.Data.Enqueue(job2);
        await job1.CompletionSource.Task.WaitAsync(cts.Token);
        await job2.CompletionSource.Task.WaitAsync(cts.Token);
        Assert.Equal(2, storage.Executed);
    }

    private class Job
    {
        public Job(bool transientFailure = false)
        {
            TransientFailure = transientFailure;
            CompletionSource = new TaskCompletionSource();
        }

        public bool TransientFailure { get; }
        public TaskCompletionSource CompletionSource { get; }
    }

    private class JobStorage<T>
    {
        public Queue<T> Data { get; } = new();
        public volatile int Executed;
    }

    private class StringJobController : OneAtATimeJobController<Job>
    {
        private readonly JobStorage<Job> storage;

        public StringJobController(JobStorage<Job> storage)
        {
            this.storage = storage;
        }

        protected override ValueTask ExecuteJob(Job data, CancellationToken cancellationToken)
        {
            if (data.TransientFailure)
            {
                data.CompletionSource.SetResult();
                return ValueTask.FromException(new TransientFailureException(null));
            }
            else
            {
                Interlocked.Increment(ref storage.Executed);
                data.CompletionSource.SetResult();
                return ValueTask.CompletedTask;
            }
        }

        protected override ValueTask<Job?> NextPendingOrDefault(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(storage.Data.TryDequeue(out Job? data) ? data : null);
        }
    }
}
