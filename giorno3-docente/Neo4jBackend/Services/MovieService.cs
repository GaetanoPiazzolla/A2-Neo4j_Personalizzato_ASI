using Neo4j.Driver;
using Neo4j.Driver.Mapping;
using Neo4jBackend.Models;

namespace Neo4jBackend.Services;

public class MovieService(IGraphSessionFactory sessionFactory)
{
    // LIVE CODING 4.3: i film con il voto IMDb più alto.
    public Task<List<MovieDto>> GetTopRatedAsync(int limit)
    {
        throw new NotImplementedException();
    }

    // TODO LAB 4.3: la query "Movie Profile" dell'homework del Giorno 2, con $title come parametro.
    // Restituire null se il film non esiste.
    public Task<MovieProfileDto?> GetProfileAsync(string title)
    {
        throw new NotImplementedException();
    }
}
