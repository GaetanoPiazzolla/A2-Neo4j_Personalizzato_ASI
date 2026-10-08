using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jBackend.Extensions;
using Neo4jBackend.Models;
using Neo4jBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Errori e 404 in formato standard ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();
builder.Services.AddNeo4j(builder.Configuration);
builder.Services.AddExceptionHandler<Neo4jExceptionHandler>();
builder.Services.AddValidation();

// LIVE CODING 4.3d: validazione automatica dei DTO.

builder.Services.AddSingleton<MovieService>();
builder.Services.AddSingleton<PersonService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Il codice prima di next() vede la richiesta, quello dopo vede la risposta.
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next(context);
        return;
    }

    app.Logger.LogInformation("==> [API ENTER] {Method} {Path}{Query}",
        context.Request.Method, context.Request.Path, context.Request.QueryString);

    var watch = System.Diagnostics.Stopwatch.StartNew();
    await next(context);
    watch.Stop();

    // Bonus LAB 4.1: le risposte di errore escono come warning.
    if (context.Response.StatusCode >= 400)
    {
        app.Logger.LogWarning("<== [API EXIT] {Method} {Path} | Status: {StatusCode} | Time: {Elapsed}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, watch.ElapsedMilliseconds);
    }
    else
    {
        app.Logger.LogInformation("<== [API EXIT] {Method} {Path} | Status: {StatusCode} | Time: {Elapsed}ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, watch.ElapsedMilliseconds);
    }
});

app.MapGet("/api/health", () => TypedResults.Ok("ok"))
    .WithName("Health");

// Il driver è lazy: è qui che scopriamo se URI e credenziali sono giusti.
app.MapGet("/api/health/db", async Task<Results<Ok<DbHealthDto>, ProblemHttpResult>> (IDriver driver) =>
{
    try
    {
        await driver.VerifyConnectivityAsync();
        var info = await driver.GetServerInfoAsync();
        return TypedResults.Ok(new DbHealthDto(info.Address, info.Agent, info.ProtocolVersion));
    }
    // Bonus LAB 4.2: credenziali sbagliate = errore di configurazione, riprovare non serve.
    catch (AuthenticationException)
    {
        return TypedResults.Problem(title: "Configurazione del database errata", statusCode: 500);
    }
    catch (Neo4jException ex)
    {
        app.Logger.LogWarning(ex, "Health check fallito");
        return TypedResults.Problem(detail: ex.Message, statusCode: 503);
    }
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

app.MapGet("/api/movies/top", async Task<Ok<List<MovieDto>>> (int? limit, MovieService service) =>
    TypedResults.Ok(await service.GetTopRatedAsync(Math.Clamp(limit ?? 10, 1, 50))))
    .WithName("GetTopMovies");

app.MapGet("/api/movies/profile", async Task<Results<Ok<MovieProfileDto>, NotFound, ProblemHttpResult>> (
    string title, MovieService service) =>
{
    var profile = await service.GetProfileAsync(title);
    return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
})
.WithName("GetMovieProfile");

// Homework: profilo di una persona.
app.MapGet("/api/persons/{tmdbId}", async Task<Results<Ok<PersonProfileDto>, NotFound, ProblemHttpResult>> (
    string tmdbId, PersonService service) =>
{
    var profile = await service.GetProfileAsync(tmdbId);
    return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
})
.WithName("GetPersonProfile");

app.MapPost("/api/actors", async Task<Created<PersonDto>> (CreatePersonDto input, PersonService personService) =>
{
    var person = await personService.CreateActorAsync(input);
    return TypedResults.Created("/api/persons/{person.TmdbId}", person);
}).WithName("CreatePerson");

app.MapPut("/api/persons/{tmdbId}", async Task<Results<Ok<PersonDto>, NotFound, ProblemHttpResult>> (
    string tmdbId, UpdatePersonDto input) =>
{
    await Task.CompletedTask; // segnaposto: da sostituire con la chiamata a PersonService
    return TypedResults.Problem(detail: "TODO LAB 4.3d", statusCode: 501);
})
.WithName("UpdatePerson");

// TODO LAB 4.3d: aggiungere PersonService tra i parametri: 204 se cancellata, 404 se non esiste.
app.MapDelete("/api/persons/{tmdbId}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
    string tmdbId) =>
{
    await Task.CompletedTask; // segnaposto: da sostituire con la chiamata a PersonService
    return TypedResults.Problem(detail: "TODO LAB 4.3d", statusCode: 501);
})
.WithName("DeletePerson");

// TODO LAB 4.3d (bonus): aggiungere PersonService tra i parametri e restituire la persona con la label Director (404 se non esiste).
app.MapPut("/api/persons/{tmdbId}/director", async Task<Results<Ok<PersonDto>, NotFound, ProblemHttpResult>> (
    string tmdbId) =>
{
    await Task.CompletedTask; // segnaposto: da sostituire con la chiamata a PersonService
    return TypedResults.Problem(detail: "TODO LAB 4.3d", statusCode: 501);
})
.WithName("AddDirectorLabel");

app.Run();

public partial class Program { }
