using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Neo4jBackend.Models;

namespace Neo4jBackend.Services;

public class MovieService(IGraphSessionFactory sessionFactory)
{
    public async Task<List<MovieDto>> GetTopRatedAsync(int limit)
    {
        await using var session = sessionFactory.CreateReadSession();

        // Transaction function: in caso di errore transitorio il driver riprova da solo.
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (m:Movie)
                WHERE m.imdbRating IS NOT NULL AND m.tmdbId IS NOT NULL
                RETURN m.tmdbId AS tmdbId, m.movieId AS movieId, m.title AS title,
                       m.year AS year, m.imdbRating AS imdbRating
                ORDER BY m.imdbRating DESC
                LIMIT $limit", new { limit });

            var records = await cursor.ToListAsync();
            return records.Select(r => r.AsObject<MovieDto>()).ToList();
        });
    }

    public async Task<MovieProfileDto?> GetProfileAsync(string title)
    {
        await using var session = sessionFactory.CreateReadSession();

        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (m:Movie {title: $title})
                OPTIONAL MATCH (d:Director)-[:DIRECTED]->(m)
                WITH m, collect(DISTINCT trim(d.name)) AS directors
                OPTIONAL MATCH (:User)-[r:RATED]->(m)
                WITH m, directors, round(avg(r.rating), 1) AS avgRating
                CALL (m) {
                  OPTIONAL MATCH (a:Actor)-[:ACTED_IN]->(m)
                  WITH a ORDER BY a.name LIMIT 5
                  RETURN collect(a.name) AS actors
                }
                RETURN m.title AS title, m.year AS year, directors, avgRating, actors", new { title });

            var records = await cursor.ToListAsync();
            return records.Count == 0 ? null : records[0].AsObject<MovieProfileDto>();
        });
    }

    // LIVE CODING 4.4: filtro comune con la ricerca, e SearchAsync (conteggio + pagina)
}
