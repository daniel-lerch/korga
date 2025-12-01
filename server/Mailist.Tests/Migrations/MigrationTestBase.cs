using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Mailist.Tests.Migrations;

public abstract class MigrationTestBase<TBefore, TAfter> : DatabaseTestBase where TBefore : DbContext where TAfter : DbContext
{
    protected readonly ITestOutputHelper testOutput;
    protected readonly TBefore before;
    protected readonly TAfter after;

    public MigrationTestBase(ITestOutputHelper testOutput) : base(testOutput)
    {
        this.testOutput = testOutput;
        before = serviceScope.ServiceProvider.GetRequiredService<TBefore>();
        after = serviceScope.ServiceProvider.GetRequiredService<TAfter>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TBefore>(
            optionsBuilder =>
            {
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString));
                optionsBuilder.LogTo(testOutput.WriteLine, LogLevel.Information);
            });

        services.AddDbContext<TAfter>(
            optionsBuilder =>
            {
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString));
                optionsBuilder.LogTo(testOutput.WriteLine, LogLevel.Information);
            });
    }
}
