using System.ComponentModel.DataAnnotations;

namespace Neo4jBackend.Models;

// Con AddValidation() gli attributi vengono controllati prima dell'endpoint: se falliscono, 400.
public record CreatePersonDto(
    [Required, MaxLength(20)] string TmdbId,
    [Required, MaxLength(100)] string Name,
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$")] string? Born = null
);
