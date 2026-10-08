using Neo4j.Driver;
using Neo4jBackend.Models;

namespace Neo4jBackend.Services;

// Converte i nodi e le relazioni del driver nei DTO del grafo.
public static class GraphMapping
{
    // Label espandibili: tutte hanno tmdbId come chiave. Genre e User no (troppi vicini).
    public static readonly string[] ExpandableLabels = ["Movie", "Person", "Actor", "Director"];

    public static NodeDto ToNodeDto(INode node) => new(
        node.ElementId,
        BusinessKey(node),
        node.Labels.ToList(),
        node.Properties.ToDictionary(p => p.Key, p => Sanitize(p.Value)));

    public static EdgeDto ToEdgeDto(IRelationship rel) => new(
        rel.ElementId,
        rel.StartNodeElementId,
        rel.EndNodeElementId,
        rel.Type,
        rel.Properties.ToDictionary(p => p.Key, p => Sanitize(p.Value)));

    // La chiave che le API capiscono: tmdbId per film e persone, name per i generi.
    private static string? BusinessKey(INode node) =>
        node.Properties.TryGetValue("tmdbId", out var id) ? id?.ToString()
        : node.Properties.TryGetValue("name", out var name) ? name?.ToString()
        : null;

    // Le date di Neo4j (born, died) diventano stringhe "1956-07-09" nel JSON.
    private static object? Sanitize(object? value) => value switch
    {
        LocalDate d => d.ToString(),
        ZonedDateTime z => z.ToString(),
        LocalDateTime l => l.ToString(),
        _ => value
    };
}
