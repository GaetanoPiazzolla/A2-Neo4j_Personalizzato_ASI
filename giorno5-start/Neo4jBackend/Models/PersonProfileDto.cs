namespace Neo4jBackend.Models;

// Default = null: una persona può non avere film recitati o diretti, o la data di nascita.
public record PersonProfileDto(
    string Name,
    string? Born = null,
    List<string>? ActedIn = null,
    List<string>? Directed = null
);
