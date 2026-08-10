using DotNet.Testcontainers.Builders;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace L2.Foundation.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PostgreSqlIntegrationCollection : ICollectionFixture<PostgreSqlIntegrationFixture>
{
    public const string Name = "PostgreSQL integration";
}

public sealed class PostgreSqlIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("l2_test_host")
        .WithUsername("l2_tests")
        .WithPassword("l2_tests_password")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted(
            "pg_isready", "-U", "l2_tests", "-d", "l2_test_host"))
        .Build();

    public Task InitializeAsync() => postgres.StartAsync();

    public Task DisposeAsync() => postgres.DisposeAsync().AsTask();

    public async Task<PostgreSqlDatabaseLease> CreateDatabaseAsync()
    {
        var databaseName = $"l2_test_{Guid.NewGuid():N}";
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)}";
        await command.ExecuteNonQueryAsync();

        var connectionString = new NpgsqlConnectionStringBuilder(postgres.GetConnectionString())
        {
            Database = databaseName
        }.ConnectionString;
        return new PostgreSqlDatabaseLease(this, databaseName, connectionString);
    }

    internal async Task DropDatabaseAsync(string databaseName)
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS {QuoteIdentifier(databaseName)} WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }

    private static string QuoteIdentifier(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}

public sealed class PostgreSqlDatabaseLease(
    PostgreSqlIntegrationFixture fixture,
    string databaseName,
    string connectionString) : IAsyncDisposable
{
    private PostgreSqlIntegrationFixture? fixture = fixture;

    public string ConnectionString { get; } = connectionString;

    public async ValueTask DisposeAsync()
    {
        var owner = Interlocked.Exchange(ref fixture, null);
        if (owner is not null)
        {
            await owner.DropDatabaseAsync(databaseName);
        }
    }
}
