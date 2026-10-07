using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Neo4jBackend.Services;

namespace Neo4jBackend.Extensions;

public static class Neo4jExtensions
{
    public static IServiceCollection AddNeo4j(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["Neo4j:Uri"] ?? throw new InvalidOperationException("Neo4j:Uri is missing");
        var user = configuration["Neo4j:User"] ?? throw new InvalidOperationException("Neo4j:User is missing");
        var password = configuration["Neo4j:Password"] ?? throw new InvalidOperationException("Neo4j:Password is missing");

        // Factory lambda: il container crea il driver e quindi ne fa anche il Dispose.
        services.AddSingleton(sp => GraphDatabase.Driver(uri, AuthTokens.Basic(user, password), o => o
            .WithUserAgent("asi-neo4j/1.0")
            .WithLogger(new Neo4jLoggerAdapter(sp.GetRequiredService<ILoggerFactory>()))
            .WithConnectionAcquisitionTimeout(TimeSpan.FromSeconds(15))));

        // Abilita la traduzione automatica camelCase <-> PascalCase.
        RecordObjectMapping.TranslateIdentifiers(translateCypherParameters: true);

        services.AddSingleton<IGraphSessionFactory, GraphSessionFactory>();

        return services;
    }
}
