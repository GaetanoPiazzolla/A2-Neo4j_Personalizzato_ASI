namespace Neo4jBackend.Models;

// Una pagina di risultati più il totale: al client serve per sapere quante pagine ci sono.
public record PaginatedResultDto<T>(
    List<T> Items,
    long TotalCount,
    int Skip,
    int Take
);
