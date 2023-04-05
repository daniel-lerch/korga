using Korga.Server.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Korga.Server.Tests;

public class JobQueueTests
{
    [Fact]
    public async Task TestRunPendingOnStart()
    {
        StringJobStorage storage = new();
        ServiceCollection services = new();
        services.AddSingleton(storage);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        JobQueue<StringJobController, string> jobQueue = new(serviceProvider);
        storage.Data = "asdf";
        await jobQueue.StartAsync(CancellationToken.None);
        await jobQueue.StopAsync(CancellationToken.None);
        Assert.True(storage.Executed);
    }

    [Fact]
    public async Task TestRunImmediately()
    {
        StringJobStorage storage = new();
        ServiceCollection services = new();
        services.AddSingleton(storage);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        JobQueue<StringJobController, string> jobQueue = new(serviceProvider);
        await jobQueue.StartAsync(CancellationToken.None);
        storage.Data = "asdf";
        jobQueue.EnsureRunning();
        await Task.Delay(50);
        await jobQueue.StopAsync(CancellationToken.None);
        Assert.True(storage.Executed);
    }


    private class StringJobStorage
    {
        public string? Data { get; set; }
        public bool Executed { get; set; }
    }

    private class StringJobController : IJobController<string>
    {
        private readonly StringJobStorage storage;

        public StringJobController(StringJobStorage storage)
        {
            this.storage = storage;
        }

        public ValueTask<bool> ExecuteJob(string data, CancellationToken cancellationToken)
        {
            if (data != storage.Data) throw new InvalidOperationException();
            storage.Executed = true;
            return ValueTask.FromResult(true);
        }

        public ValueTask<string?> NextPendingOrDefault(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(storage.Data);
        }
    }
}
