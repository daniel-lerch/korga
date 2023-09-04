using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Korga.Server.Tests.Migrations;

public abstract class MigrationTestBase<TBefore, TAfter> : DatabaseTestBase where TBefore : DbContext where TAfter : DbContext
{
    protected readonly TBefore before;
    protected readonly TAfter after;

    public MigrationTestBase()
    {
        before = serviceScope.ServiceProvider.GetRequiredService<TBefore>();
        after = serviceScope.ServiceProvider.GetRequiredService<TAfter>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TBefore>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));

        services.AddDbContext<TAfter>(
            optionsBuilder => optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(shortConnectionString)));
    }
}
