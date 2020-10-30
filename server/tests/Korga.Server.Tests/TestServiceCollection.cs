using Korga.Server.Database;
using Korga.Server.Extensions;
using Korga.Server.Services;
using Korga.Server.Tests.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Korga.Server.Tests
{
    public static class TestServiceCollection
    {
        public static IServiceCollection CreateDefault()
        {
            var configuration = new ConfigurationBuilder()
                .AddKorga()
                .Build();

            var services = new ServiceCollection();
            services.ConfigureKorga(configuration);
            services.AddSingleton<LdapService>();
            services.AddDbContext<DatabaseContext>();
            return services;
        }
    }
}
