namespace Neo4jBackend.Models;

// I default rendono i campi opzionali per AsObject<T> (es. film senza voti).
public record MovieProfileDto(
    string Title,
    int? Year = null,
    List<string>? Directors = null,
    double? AvgRating = null,
    List<string>? Actors = null
);
