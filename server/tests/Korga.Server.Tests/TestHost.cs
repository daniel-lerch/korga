using Korga.Server.Database;
using Korga.Server.Extensions;
using Korga.Server.Services;
using Korga.Server.Tests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace Korga.Server.Tests
{
    public static class TestHost
    {
        public static IServiceCollection CreateServiceCollection()
        {
            var configuration = new ConfigurationBuilder()
                .AddKorga()
                .Build();

            var services = new ServiceCollection();
            services.ConfigureKorga(configuration);
            services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
            services.AddSingleton<LdapService>();
            services.AddDbContext<DatabaseContext>();
            return services;
        }

        public static TestServer CreateTestServer(IEnumerable<KeyValuePair<string, string?>>? configuration = null)
        {
            return new TestServer(new WebHostBuilder()
                // Working directory: tests/Korga.Server.Tests/bin/Release/net5.0
                .UseWebRoot("../../../../../src/Korga.Server/wwwroot")
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddKorga();

                    if (configuration != null)
                    {
                        builder.AddInMemoryCollection(configuration);
                    }
                })
                .UseStartup<Startup>());
        }
    }
}
