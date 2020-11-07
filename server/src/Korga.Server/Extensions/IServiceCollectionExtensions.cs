using Korga.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Korga.Server.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureKorga(this IServiceCollection services, IConfiguration configuration)
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

            return services;
        }
    }
}
