using Korga.Server.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace Korga.Server.Extensions;

public static class HostExtensions
{
    public static IHost MigrateDatabase(this IHost host)
    {
        var options = host.Services.GetRequiredService<IOptions<DatabaseOptions>>();

        if (options.Value.MigrateOnStartup)
        {
            using IServiceScope scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            IEnumerable<string> pendingMigrations = database.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying the following migrations: {Migrations}", string.Join(", ", pendingMigrations));
                database.Database.Migrate();
            }
            else
            {
                logger.LogInformation("Database migration on startup is enabled: No pending migrations");
            }
        }

        return host;
    }
}
