using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jBackend.Extensions;
using Neo4jBackend.Models;
using Neo4jBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// LIVE CODING 4.1: ProblemDetails.

builder.Services.AddNeo4j(builder.Configuration);

// LIVE CODING 4.2 ERRORI: registrare Neo4jExceptionHandler.

// LIVE CODING 4.3: registrare MovieService.

var app = builder.Build();

// LIVE CODING 4.1: UseExceptionHandler e UseStatusCodePages.

// LIVE CODING 4.1: middleware minimo con la riga di ingresso (app.Use e next).

// TODO LAB 4.1: completare il middleware: riga di uscita con status code e millisecondi,
// solo per le chiamate /api.

// LIVE CODING 4.1: sostituire con GET /api/health.
app.MapGet("/", () => "Hello World!");

// TODO LAB 4.2: aggiungere IDriver tra i parametri, verificare la connessione e restituire DbHealthDto.
// Se Neo4j non risponde: 503 con TypedResults.Problem.
app.MapGet("/api/health/db", async Task<Results<Ok<DbHealthDto>, ProblemHttpResult>> () =>
{
    await Task.CompletedTask; // segnaposto: da sostituire con le chiamate al driver
    return TypedResults.Problem(detail: "TODO LAB 4.2", statusCode: 501);
})
.WithName("HealthDb");

// Endpoint di prova per il blocco 4.2 ERRORI: falliscono apposta.
app.MapGet("/api/debug/broken-query", async ([FromServices] IDriver driver) =>
{
    await driver.ExecutableQuery("MATCH (m:Movie RETURN m").ExecuteAsync();
    return TypedResults.Ok();
});

app.MapGet("/api/debug/duplicate-genre", async ([FromServices] IDriver driver) =>
{
    await driver.ExecutableQuery("CREATE (:Genre {name: 'Action'})").ExecuteAsync();
    return TypedResults.Ok();
});

// LIVE CODING 4.3: GET /api/movies/top?limit=5

// TODO LAB 4.3: aggiungere MovieService tra i parametri e restituire il profilo del film (404 se non esiste).
app.MapGet("/api/movies/profile", async Task<Results<Ok<MovieProfileDto>, NotFound, ProblemHttpResult>> (string title) =>
{
    await Task.CompletedTask; // segnaposto: da sostituire con la chiamata a MovieService
    return TypedResults.Problem(detail: "TODO LAB 4.3", statusCode: 501);
})
.WithName("GetMovieProfile");

app.Run();

public partial class Program { }
