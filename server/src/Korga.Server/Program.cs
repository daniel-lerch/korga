using Korga.Server.Commands;
using Korga.Server.Extensions;
using Korga.Server.Utilities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Korga.Server;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
#if DEBUG
        if (Debugger.IsAttached && !NativeMethods.IsRunningInProcessIIS())
        {
            Console.Write("Korga server is running in debug mode. Please enter your command: ");
            args = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            Console.WriteLine();
        }
#endif
        if (args.Length == 0)
        {
            await CreateWebHostBuilder(args).Build()
                // Automatic database migration is done here right after building the host and not in Startup.Configure
                // to make sure no other other hosted services can start while migrations are not yet completed.
                .MigrateDatabase()
                .RunAsync();
            return Environment.ExitCode;
        }
        else
        {
            return await CreateAndRunCommandLine(args);
        }
    }

    private static async Task<int> CreateAndRunCommandLine(string[] args)
    {
        IServiceScope? scope = null;
        try
        {
            return await CreateCliHostBuilder().RunCommandLineApplicationAsync<KorgaCommand>(args, app =>
            {
                // This method disposes the host after shutdown. Therefore, it might be dangerous to dispose the scope after that.
                var scope = app.CreateScope();
                app.Conventions.UseConstructorInjection(scope.ServiceProvider);
            });
        }
        catch (UnrecognizedCommandParsingException) // Host integration of v3.0.0 does not support disabling this exception
        {
            return 1;
        }
        finally
        {
            scope?.Dispose();
        }
    }

    private static IHostBuilder CreateWebHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    private static IHostBuilder CreateCliHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            })
            .ConfigureServices((context, services) =>
            {
                services.AddKorgaOptions(context.Configuration);
				services.AddSingleton(PhysicalConsole.Singleton);
                services.AddKorgaMySqlDatabase();
            });
}
