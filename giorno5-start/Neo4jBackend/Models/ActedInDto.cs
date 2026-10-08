namespace Neo4jBackend.Models;

// Risposta del PUT acted-in: chi, in quale film, con quali ruoli.
public record ActedInDto(
    string Person,
    string Movie,
    List<string>? Roles = null
);
