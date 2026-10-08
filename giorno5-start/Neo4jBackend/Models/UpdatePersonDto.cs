using System.ComponentModel.DataAnnotations;

namespace Neo4jBackend.Models;

// Il tmdbId non è nel body: arriva dall'URL (PUT /api/persons/{tmdbId}).
public record UpdatePersonDto(
    [Required, MaxLength(100)] string Name,
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$")] string? Born = null
);
