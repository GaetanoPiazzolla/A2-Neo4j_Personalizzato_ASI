using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Neo4jBackend.Extensions;
using Neo4jBackend.Models;
using Neo4jBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNeo4j(builder.Configuration);
builder.Services.AddProblemDetails();

// LIVE CODING 4.2 ERRORI: registrare Neo4jExceptionHandler.

// LIVE CODING 4.3: registrare MovieService.

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

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

// LIVE CODING 4.1: sostituire con GET /api/health.
app.MapGet("/api/health", () =>
        TypedResults.Ok("Ok")).WithName("Health");

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
