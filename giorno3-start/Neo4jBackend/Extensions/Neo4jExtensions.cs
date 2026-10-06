using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Neo4jBackend.Services;

namespace Neo4jBackend.Extensions;

public static class Neo4jExtensions
{
    public static IServiceCollection AddNeo4j(this IServiceCollection services, IConfiguration configuration)
    {
        // LIVE CODING 4.2: leggere la configurazione e registrare IDriver come singleton.

        // LIVE CODING 4.3: mapping camelCase <-> PascalCase e IGraphSessionFactory.

        return services;
    }
}
