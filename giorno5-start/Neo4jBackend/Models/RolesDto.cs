using System.ComponentModel.DataAnnotations;

namespace Neo4jBackend.Models;

// Body del PUT acted-in: i ruoli sono proprietà della relazione, non della persona o del film.
public record RolesDto([Required] List<string> Roles);
