using Korga.Extensions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace Korga.Tests;

public static class TestHost
{
    public static IServiceCollection CreateServiceCollection()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            // Integration tests work without user secrets like in a CI pipeline
            .Build();

        var services = new ServiceCollection();
        services.AddKorgaOptions(configuration);
        services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
        services.AddKorgaMySqlDatabase();
        return services;
    }

    public static TestServer CreateTestServer(IEnumerable<KeyValuePair<string, string?>>? configuration = null)
    {
        return new TestServer(new WebHostBuilder()
            // Working directory: Korga.Tests/bin/Debug/net8.0
            .UseWebRoot("../../../../Korga/wwwroot")
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", optional: false);
                // Integration tests work without user secrets like in a CI pipeline

                if (configuration != null)
                {
                    builder.AddInMemoryCollection(configuration);
                }
            })
            .UseStartup<Startup>());
    }

    public static IHostBuilder CreateCliHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddUserSecrets<Program>();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddKorgaOptions(context.Configuration);
                services.AddSingleton<IConsole>(NullConsole.Singleton);
                services.AddKorgaMySqlDatabase();
            });
    }
}
