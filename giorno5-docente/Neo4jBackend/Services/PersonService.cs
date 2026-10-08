using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Neo4jBackend.Models;

namespace Neo4jBackend.Services;

public class PersonService(IGraphSessionFactory sessionFactory)
{
    public async Task<PersonProfileDto?> GetProfileAsync(string tmdbId)
    {
        await using var session = sessionFactory.CreateReadSession();

        return await session.ExecuteReadAsync(async tx =>
        {
            // Il WITH chiude la prima aggregazione: senza, film recitati e diretti si moltiplicano.
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {tmdbId: $tmdbId})
                OPTIONAL MATCH (p)-[:ACTED_IN]->(a:Movie)
                WITH p, a ORDER BY a.year
                WITH p, collect(DISTINCT a.title) AS actedIn
                OPTIONAL MATCH (p)-[:DIRECTED]->(d:Movie)
                RETURN p.name AS name, toString(p.born) AS born, actedIn,
                       collect(DISTINCT d.title) AS directed", new { tmdbId });

            var records = await cursor.ToListAsync();
            return records.Count == 0 ? null : records[0].AsObject<PersonProfileDto>();
        });
    }

    public async Task<PersonDto> CreateActorAsync(CreatePersonDto input)
    {
        await using var session = sessionFactory.CreateWriteSession();

        return await session.ExecuteWriteAsync(async tx =>
        {
            // CREATE e non MERGE: un tmdbId già presente deve dare 409 (constraint), non un finto 201.
            var cursor = await tx.RunAsync(@"
                CREATE (p:Person:Actor {tmdbId: $tmdbId, name: $name, born: date($born)})
                RETURN p.tmdbId AS tmdbId, p.name AS name, toString(p.born) AS born, labels(p) AS labels",
                new { input.TmdbId, input.Name, input.Born });

            var record = await cursor.SingleAsync();
            return record.AsObject<PersonDto>();
        });
    }

    public async Task<PersonDto?> UpdateAsync(string tmdbId, UpdatePersonDto input)
    {
        await using var session = sessionFactory.CreateWriteSession();

        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {tmdbId: $tmdbId})
                SET p.name = $name, p.born = date($born)
                RETURN p.tmdbId AS tmdbId, p.name AS name, toString(p.born) AS born, labels(p) AS labels",
                new { tmdbId, input.Name, input.Born });

            var records = await cursor.ToListAsync();
            return records.Count == 0 ? null : records[0].AsObject<PersonDto>();
        });
    }

    public async Task<bool> DeleteAsync(string tmdbId)
    {
        await using var session = sessionFactory.CreateWriteSession();

        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {tmdbId: $tmdbId})
                DETACH DELETE p", new { tmdbId });

            var summary = await cursor.ConsumeAsync();
            return summary.Counters.NodesDeleted > 0;
        });
    }

    // Bonus LAB 4.3d: la stessa persona può avere più ruoli, basta aggiungere la label.
    public async Task<PersonDto?> AddDirectorLabelAsync(string tmdbId)
    {
        await using var session = sessionFactory.CreateWriteSession();

        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {tmdbId: $tmdbId})
                SET p:Director
                RETURN p.tmdbId AS tmdbId, p.name AS name, toString(p.born) AS born, labels(p) AS labels",
                new { tmdbId });

            var records = await cursor.ToListAsync();
            return records.Count == 0 ? null : records[0].AsObject<PersonDto>();
        });
    }

    public async Task<ActedInDto?> AddActedInAsync(string tmdbId, string movieTmdbId, List<string> roles)
    {
        await using var session = sessionFactory.CreateWriteSession();

        return await session.ExecuteWriteAsync(async tx =>
        {
            // MERGE sulla relazione: rieseguito non crea un secondo arco, aggiorna solo i ruoli.
            var cursor = await tx.RunAsync(@"
                MATCH (p:Person {tmdbId: $tmdbId})
                MATCH (m:Movie {tmdbId: $movieTmdbId})
                MERGE (p)-[r:ACTED_IN]->(m)
                SET r.roles = $roles
                RETURN p.name AS person, m.title AS movie, r.roles AS roles",
                new { tmdbId, movieTmdbId, roles });

            var records = await cursor.ToListAsync();
            return records.Count == 0 ? null : records[0].AsObject<ActedInDto>();
        });
    }

    // Si cancella solo l'arco, i due nodi restano.
    public async Task<bool> RemoveActedInAsync(string tmdbId, string movieTmdbId)
    {
        await using var session = sessionFactory.CreateWriteSession();

        return await session.ExecuteWriteAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (:Person {tmdbId: $tmdbId})-[r:ACTED_IN]->(:Movie {tmdbId: $movieTmdbId})
                DELETE r", new { tmdbId, movieTmdbId });

            var summary = await cursor.ConsumeAsync();
            return summary.Counters.RelationshipsDeleted > 0;
        });
    }
}
