using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Neo4jBackend.Services;

namespace Neo4jBackend.Extensions;

public static class Neo4jExtensions
{
    public static IServiceCollection AddNeo4j(this IServiceCollection services, IConfiguration configuration)
    {
        // LIVE CODING 4.2: leggere la configurazione e registrare IDriver come singleton.
        var uri = configuration.GetValue<string>("Neo4j:Uri") ??
                  throw new InvalidOperationException("Neo4j:Uri is not configured");
        var user = configuration.GetValue<string>("Neo4j:User") ??
                    throw new InvalidOperationException("Neo4j:User is not configured");
        var password = configuration.GetValue<string>("Neo4j:Password") ??
                       throw new InvalidOperationException("Neo4j:Password is not configured");

        services.AddSingleton(sp =>
            GraphDatabase.Driver(uri, AuthTokens.Basic(user, password), o =>
                o.WithUserAgent("asi-neo4j/1.0")
                    .WithLogger(new Neo4jLoggerAdapter(sp.GetRequiredService<ILoggerFactory>()))
                    .WithConnectionAcquisitionTimeout(TimeSpan.FromSeconds(10)))
            );

        // LIVE CODING 4.3: mapping camelCase <-> PascalCase e IGraphSessionFactory.

        return services;
    }
}
