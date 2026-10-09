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

// Controlla gli attributi dei DTO (Required, MaxLength...) prima dell'endpoint: se falliscono, 400.
builder.Services.AddValidation();

builder.Services.AddSingleton<MovieService>();
builder.Services.AddSingleton<PersonService>();
builder.Services.AddSingleton<GraphService>();

// Descrizione OpenAPI degli endpoint: il frontend ne genera il client tipizzato.
builder.Services.AddOpenApi();

// Il frontend gira su un'altra origine (porta 5173): senza CORS il browser blocca le risposte.
builder.Services.AddCors(options => options.AddPolicy("DevCors", policy => policy
    .WithOrigins("http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // GET /openapi/v1.json
    app.UseCors("DevCors");
}

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

app.MapPost("/api/actors", async Task<Created<PersonDto>> (CreatePersonDto input, PersonService service) =>
{
    var person = await service.CreateActorAsync(input);
    return TypedResults.Created($"/api/persons/{person.TmdbId}", person);
})
.WithName("CreateActor");

app.MapPut("/api/persons/{tmdbId}", async Task<Results<Ok<PersonDto>, NotFound, ProblemHttpResult>> (
    string tmdbId, UpdatePersonDto input, PersonService service) =>
{
    var person = await service.UpdateAsync(tmdbId, input);
    return person is null ? TypedResults.NotFound() : TypedResults.Ok(person);
})
.WithName("UpdatePerson");

app.MapDelete("/api/persons/{tmdbId}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
    string tmdbId, PersonService service) =>
{
    var deleted = await service.DeleteAsync(tmdbId);
    return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
})
.WithName("DeletePerson");

// Bonus LAB 4.3d
app.MapPut("/api/persons/{tmdbId}/director", async Task<Results<Ok<PersonDto>, NotFound>> (string tmdbId, PersonService service) =>
{
    var person = await service.AddDirectorLabelAsync(tmdbId);
    return person is null ? TypedResults.NotFound() : TypedResults.Ok(person);
})
.WithName("AddDirectorLabel");

// LIVE CODING 4.4: GET /api/movies paginato, con ricerca

// LIVE CODING 4.4b: GET /api/graph/expand/{label}/{id}

// La relazione si identifica con i suoi due estremi; i ruoli sono proprietà dell'arco.
app.MapPut("/api/persons/{tmdbId}/acted-in/{movieTmdbId}", async Task<Results<Ok<ActedInDto>, NotFound, ProblemHttpResult>> (
    string tmdbId, string movieTmdbId, RolesDto input, PersonService service) =>
{
    var actedIn = await service.AddActedInAsync(tmdbId, movieTmdbId, input.Roles);
    return actedIn is null ? TypedResults.NotFound() : TypedResults.Ok(actedIn);
})
.WithName("AddActedInRelation");

app.MapDelete("/api/persons/{tmdbId}/acted-in/{movieTmdbId}", async Task<Results<NoContent, NotFound, ProblemHttpResult>> (
    string tmdbId, string movieTmdbId, PersonService service) =>
{
    var deleted = await service.RemoveActedInAsync(tmdbId, movieTmdbId);
    return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
})
.WithName("RemoveActedInRelation");

app.MapGet("/api/movies", async Task<Ok<PaginatedResultDto<MovieDto>>> (
            int? skip, int? take, string? search, MovieService service) =>
        TypedResults.Ok(await service.SearchAsync(Math.Max(skip ?? 0, 0), Math.Clamp(take ?? 20, 1, 100), search)))
    .WithName("GetMovies");


app.Run();

public partial class Program { }
