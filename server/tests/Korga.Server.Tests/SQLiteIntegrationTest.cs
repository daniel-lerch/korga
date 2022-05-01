using Korga.Server.Database;
using Korga.Server.Extensions;
using Korga.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Korga.Server.Tests;

public abstract class SQLiteIntegrationTest : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly IServiceScope serviceScope;

    public SQLiteIntegrationTest()
    {
        connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        TestServer = new TestServer(new WebHostBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddKorgaOptions(context.Configuration);
                services.AddDbContext<DatabaseContext>(optionsBuilder => optionsBuilder.UseSqlite(connection));
                services.AddScoped<EventRegistrationService>();
                services.AddScoped<EventSampleDataService>();
                services.AddControllers();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }));

        TestClient = TestServer.CreateClient();

        serviceScope = TestServer.Services.CreateScope();
        Database = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Database.Database.EnsureDeleted();
        Database.Database.EnsureCreated();
    }

    protected TestServer TestServer { get; }

    protected HttpClient TestClient { get; }

    protected DatabaseContext Database { get; }

    protected T GetRequiredService<T>() where T : notnull
    {
        return serviceScope.ServiceProvider.GetRequiredService<T>();
    }

    public void Dispose()
    {
        serviceScope.Dispose();
        TestClient.Dispose();
        TestServer.Dispose();
        connection.Dispose();
    }
}
