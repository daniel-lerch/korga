using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using Xunit;

namespace Mailist.Tests;

public abstract class DatabaseTestBase : IDisposable
{
    protected readonly string databaseName;
    protected readonly string shortConnectionString;
    protected readonly string connectionString;
    protected readonly MySqlConnection connection;
    protected readonly ServiceProvider serviceProvider;
    protected readonly IServiceScope serviceScope;
    protected readonly DatabaseContext databaseContext;

    public DatabaseTestBase(ITestOutputHelper? testOutput = null)
    {
        databaseName = "Korga_" + GetType().Name.Replace("Tests", string.Empty);
        shortConnectionString = "Server=localhost;Port=3306;User=root;Password=root;";
        connectionString = $"Server=localhost;Port=3306;Database={databaseName};User=root;Password=root;";

        // Create test database
        connection = new(shortConnectionString);
        connection.Open();
        CreateEmptyDatabase(connection, databaseName);
        connection.ChangeDatabase(databaseName);

        // Create DatabaseContext for migrations
        var serviceCollection = new ServiceCollection()
            .AddDbContext<DatabaseContext>(optionsBuilder =>
            {
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(shortConnectionString),
                    builder => builder.EnableRetryOnFailure());
                if (testOutput != null)
                    optionsBuilder.LogTo(testOutput.WriteLine, LogLevel.Information);
            });
        ConfigureServices(serviceCollection);
        serviceProvider = serviceCollection.BuildServiceProvider();

        serviceScope = serviceProvider.CreateScope();
        databaseContext = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
    }

    public void Dispose()
    {
        serviceScope.Dispose();
        serviceProvider.Dispose();

        DropDatabase(connection, databaseName);

        connection.Close();
        connection.Dispose();
    }

    protected virtual void ConfigureServices(IServiceCollection services) { }

    private static void CreateEmptyDatabase(MySqlConnection connection, string databaseName)
    {
        using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"DROP DATABASE IF EXISTS {databaseName}";
            command.ExecuteNonQuery();
        }
        using (MySqlCommand command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE {databaseName}";
            command.ExecuteNonQuery();
        }
    }

    private static void DropDatabase(MySqlConnection connection, string databaseName)
    {
        using MySqlCommand command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE {databaseName}";
        command.ExecuteNonQuery();
    }
}
