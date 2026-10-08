namespace Neo4jBackend.Models;

// Nodi e archi nel formato che il frontend passa a vis-network.
public record GraphPayloadDto(List<NodeDto> Nodes, List<EdgeDto> Edges);

// Id = elementId (unico nel database, per vis-network); BusinessKey = tmdbId o name (per le API).
public record NodeDto(
    string Id,
    string? BusinessKey,
    List<string> Labels,
    Dictionary<string, object?> Properties
);

// Source e Target sono gli elementId dei due nodi.
public record EdgeDto(
    string Id,
    string Source,
    string Target,
    string Type,
    Dictionary<string, object?> Properties
);
