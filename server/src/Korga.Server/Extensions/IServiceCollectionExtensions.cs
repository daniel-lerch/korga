using Korga.Server.Configuration;
using Korga.Server.Database;
using Korga.Server.EmailRelay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Korga.Server.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddKorgaOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations();
        services.AddOptions<HostingOptions>()
            .Bind(configuration.GetSection("Hosting"))
            .ValidateDataAnnotations();
        services.AddOptions<LdapOptions>()
            .Bind(configuration.GetSection("Ldap"))
            .ValidateDataAnnotations();
        services.AddOptions<EmailRelayOptions>()
            .Bind(configuration.GetSection("EmailRelay"))
            .ValidateDataAnnotations();

        return services;
    }

    public static IServiceCollection AddKorgaMySqlDatabase(this IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>((services, optionsBuilder) =>
        {
            var options = services.GetRequiredService<IOptions<DatabaseOptions>>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.UseMySql(options.Value.ConnectionString, ServerVersion.AutoDetect(options.Value.ConnectionString));
        });

        return services;
    }
}
