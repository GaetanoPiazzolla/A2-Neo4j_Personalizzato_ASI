namespace Neo4jBackend.Models;

// Risposta delle scritture: Labels mostra il multi-label (Person + Actor/Director).
public record PersonDto(
    string TmdbId,
    string Name,
    string? Born = null,
    List<string>? Labels = null
);
