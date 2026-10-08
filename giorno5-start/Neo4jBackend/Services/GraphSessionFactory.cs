using Neo4j.Driver;

namespace Neo4jBackend.Services;

public interface IGraphSessionFactory
{
    IAsyncSession CreateReadSession();
    IAsyncSession CreateWriteSession();
    Task<EagerResult<IReadOnlyList<IRecord>>> ExecuteReadQueryAsync(string query, object? parameters = null);
    Task<EagerResult<IReadOnlyList<IRecord>>> ExecuteWriteQueryAsync(string query, object? parameters = null);
    Task VerifyConnectivityAsync();
}

public class GraphSessionFactory(IDriver driver, IConfiguration configuration) : IGraphSessionFactory
{
    private readonly IBookmarkManager _bookmarkManager = GraphDatabase.BookmarkManagerFactory.NewBookmarkManager();
    private readonly string _database = configuration["Neo4j:Database"] ?? "neo4j";

    public IAsyncSession CreateReadSession()
    {
        // in case of multiple replicas => better traffic pattern
        return driver.AsyncSession(config => config
            .WithDefaultAccessMode(AccessMode.Read)
            .WithBookmarkManager(_bookmarkManager)
            .WithDatabase(_database));
    }

    public IAsyncSession CreateWriteSession()
    {
        return driver.AsyncSession(config => config
            .WithDefaultAccessMode(AccessMode.Write)
            .WithBookmarkManager(_bookmarkManager)
            .WithDatabase(_database));
    }

    public Task<EagerResult<IReadOnlyList<IRecord>>> ExecuteReadQueryAsync(string query, object? parameters = null)
    {
        return driver.ExecutableQuery(query)
            .WithParameters(parameters ?? new { })
            .WithConfig(new QueryConfig(RoutingControl.Readers, database: _database, bookmarkManager: _bookmarkManager))
            .ExecuteAsync();
    }

    public Task<EagerResult<IReadOnlyList<IRecord>>> ExecuteWriteQueryAsync(string query, object? parameters = null)
    {
        return driver.ExecutableQuery(query)
            .WithParameters(parameters ?? new { })
            .WithConfig(new QueryConfig(RoutingControl.Writers, database: _database, bookmarkManager: _bookmarkManager))
            .ExecuteAsync();
    }

    public Task VerifyConnectivityAsync()
    {
        return driver.VerifyConnectivityAsync();
    }
}
