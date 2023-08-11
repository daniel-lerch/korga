using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;

namespace Korga.Server.Tests.Migrations;

public abstract class MigrationTest : IDisposable
{
    private readonly ServiceProvider serviceProvider;
    private readonly IServiceScope serviceScope;

    protected readonly string databaseName;
    protected readonly MySqlConnection connection;
    protected readonly DatabaseContext databaseContext;

    public MigrationTest(string testName)
    {
        databaseName = $"Korga_{testName}";
        string shortConnectionString = "Server=localhost;Port=3306;User=root;Password=root;";
        string connectionString = $"Server=localhost;Port=3306;Database={databaseName};User=root;Password=root;";

        // Create test database
        connection = new(shortConnectionString);
        connection.Open();
        CreateEmptyDatabase(connection, databaseName);
        connection.ChangeDatabase(databaseName);

        // Create DatabaseContext for migrations
        serviceProvider = new ServiceCollection()
            .AddDbContext<DatabaseContext>(optionsBuilder =>
            {
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(shortConnectionString),
                    builder =>
                    {
                        builder.MigrationsAssembly($"{nameof(Korga)}.{nameof(Server)}");
                        builder.EnableRetryOnFailure();
                    });
            })
            .BuildServiceProvider();

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
