using Microsoft.AspNetCore.Diagnostics;
using Neo4j.Driver;

namespace Neo4jBackend.Extensions;

public class Neo4jExceptionHandler(IProblemDetailsService problemDetails, ILogger<Neo4jExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        // LIVE CODING 4.2 ERRORI: tradurre le eccezioni del driver in status HTTP.

        // TODO LAB 4.2 ERRORI: 409 per i vincoli violati, niente dettagli interni nei 500, log dell'errore.

        return false;
    }
}
