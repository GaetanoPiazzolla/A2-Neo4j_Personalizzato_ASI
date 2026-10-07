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

    // LIVE CODING 4.3d: crea una persona con le label Person e Actor.
    public Task<PersonDto> CreateActorAsync(CreatePersonDto input)
    {
        throw new NotImplementedException();
    }

    // TODO LAB 4.3d: MATCH della persona per tmdbId, SET di name e born, RETURN come in CreateActorAsync.
    // Restituire null se la persona non esiste.
    public Task<PersonDto?> UpdateAsync(string tmdbId, UpdatePersonDto input)
    {
        throw new NotImplementedException();
    }

    // TODO LAB 4.3d: cancellare la persona e le sue relazioni.
    // Restituire false se non esisteva (Counters del summary).
    public Task<bool> DeleteAsync(string tmdbId)
    {
        throw new NotImplementedException();
    }
}
