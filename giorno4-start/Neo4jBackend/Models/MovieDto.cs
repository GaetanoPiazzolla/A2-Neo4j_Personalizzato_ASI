namespace Neo4jBackend.Models;

// Record C# mappato dai risultati Cypher (usando AsObject<T> e RecordObjectMapping)
// I valori di default rendono i parametri OPZIONALI per il mapper: senza,
// AsObject<T> solleva MappingFailedException sul primo film a cui manca la
// proprieta' (nel dataset ~29 film non hanno "year").
public record MovieDto(
    string TmdbId,
    string MovieId,
    string Title,
    int? Year = null,
    string? Plot = null,
    string? Poster = null,
    double? ImdbRating = null,
    List<string>? Genres = null
);
