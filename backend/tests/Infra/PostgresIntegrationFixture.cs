using Dapper;
using Infra;
using Npgsql;
using Portal.Infra;
using Testcontainers.PostgreSql;

namespace sso.repositories;

[CollectionDefinition("database")] 
public sealed class DatabaseCollection : ICollectionFixture<PostgresIntegrationFixture> { }

public sealed class PostgresIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public PostgresIntegrationFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("sso_integration")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await InitializeSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.ExecuteAsync(DatabaseScripts.TruncateAll);
    }

    public IUnitOfWork CreateUnitOfWork()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        return new UnitOfWork(connection);
    }

    public async Task<int> ExecuteScalarAsync(string sql, object? param = null)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<int>(sql, param);
    }

    public async Task ExecuteAsync(string sql, object? param = null)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.ExecuteAsync(sql, param);
    }

    private async Task InitializeSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.ExecuteAsync(DatabaseScripts.CreateSchema);
        await connection.ExecuteAsync(DatabaseScripts.TruncateAll);
    }
}
