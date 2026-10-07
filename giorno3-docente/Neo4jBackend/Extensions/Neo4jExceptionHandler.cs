using Microsoft.AspNetCore.Diagnostics;
using Neo4j.Driver;

namespace Neo4jBackend.Extensions;

public class Neo4jExceptionHandler(IProblemDetailsService problemDetails, ILogger<Neo4jExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not Neo4jException neo4j) return false;

        var (status, title) = neo4j switch
        {
            ServiceUnavailableException => (503, "Database non raggiungibile"),
            { IsRetriable: true } => (503, "Errore temporaneo, riprova"),
            { Code: "Neo.ClientError.Schema.ConstraintValidationFailed" } => (409, "Il dato esiste già"),
            _ => (500, "Errore del database")
        };

        logger.LogError(exception, "Neo4j {Code} -> HTTP {Status}", neo4j.Code, status);

        context.Response.StatusCode = status;

        // Bonus LAB 4.2 ERRORI: dice al client dopo quanti secondi riprovare.
        if (status == 503)
        {
            context.Response.Headers.RetryAfter = "5";
        }
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            // Il messaggio di un 500 può contenere la query: resta nei log, non va al client.
            ProblemDetails = { Status = status, Title = title, Detail = status < 500 ? neo4j.Message : null }
        });
    }
}
