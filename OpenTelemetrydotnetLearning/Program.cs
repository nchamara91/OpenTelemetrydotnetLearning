using OpenTelemetrydotnetLearning;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry with OTLP exporter
var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
var otlpApiKey = builder.Configuration["Otlp:ApiKey"];

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OpenTelemetrydotnetLearning"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(options =>
            {
                if (!string.IsNullOrEmpty(otlpEndpoint) && Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var uri))
                {
                    options.Endpoint = uri;
                }
                
                if (!string.IsNullOrEmpty(otlpApiKey))
                {
                    options.Headers = $"x-api-key={otlpApiKey}";
                }
            });
    });

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure OpenTelemetry with Console Exporter
var serviceName = "OpenTelemetryDotnetLearning";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: serviceName,
        serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

// Get logger factory and create loggers
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var moviesLogger = loggerFactory.CreateLogger("MoviesEndpoints");
var weatherLogger = loggerFactory.CreateLogger("WeatherEndpoints");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Sample movie data
var movies = new List<Movie>
{
    new(1, "Inception", "Christopher Nolan", 2010, "Sci-Fi"),
    new(2, "The Matrix", "The Wachowskis", 1999, "Sci-Fi"),
    new(3, "Interstellar", "Christopher Nolan", 2014, "Sci-Fi"),
    new(4, "The Godfather", "Francis Ford Coppola", 1972, "Crime"),
    new(5, "Pulp Fiction", "Quentin Tarantino", 1994, "Crime"),
    new(6, "The Dark Knight", "Christopher Nolan", 2008, "Action"),
    new(7, "Forrest Gump", "Robert Zemeckis", 1994, "Drama"),
    new(8, "The Shawshank Redemption", "Frank Darabont", 1994, "Drama")
};

// Get all movies
app.MapGet("/movies", () =>
{
    moviesLogger.LogInformation("Retrieving all movies. Total count: {MovieCount}", movies.Count);
    return movies;
})
    .WithName("GetAllMovies")
    .WithDescription("Get all movies");

// Get movie by ID
app.MapGet("/movies/{id:int}", (int id) =>
{
    moviesLogger.LogInformation("Searching for movie with ID: {MovieId}", id);
    var movie = movies.FirstOrDefault(m => m.Id == id);
    if (movie is not null)
    {
        moviesLogger.LogInformation("Found movie: {MovieTitle} by {MovieDirector}", movie.Title, movie.Director);
        return Results.Ok(movie);
    }
    moviesLogger.LogWarning("Movie with ID: {MovieId} not found", id);
    return Results.NotFound();
})
    .WithName("GetMovieById")
    .WithDescription("Get a movie by its ID");

// Search movies by title, director, or genre
app.MapGet("/movies/search", (string? title, string? director, string? genre) =>
{
    moviesLogger.LogInformation("Searching movies with filters - Title: {Title}, Director: {Director}, Genre: {Genre}",
        title ?? "any", director ?? "any", genre ?? "any");
    
    var result = movies.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(title))
    {
        result = result.Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(director))
    {
        result = result.Where(m => m.Director.Contains(director, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(genre))
    {
        result = result.Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
    }

    var resultList = result.ToList();
    moviesLogger.LogInformation("Search completed. Found {ResultCount} movies matching the criteria", resultList.Count);
    return resultList;
})
    .WithName("SearchMovies")
    .WithDescription("Search movies by title, director, or genre");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    weatherLogger.LogInformation("Generating weather forecast for the next {DayCount} days", 5);
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    weatherLogger.LogInformation("Weather forecast generated successfully with {ForecastCount} entries", forecast.Length);
    return forecast;
})
.WithName("GetWeatherForecast");



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
