using OpenTelemetrydotnetLearning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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
app.MapGet("/movies", () => movies)
    .WithName("GetAllMovies")
    .WithDescription("Get all movies");

// Get movie by ID
app.MapGet("/movies/{id:int}", (int id) =>
{
    var movie = movies.FirstOrDefault(m => m.Id == id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
})
    .WithName("GetMovieById")
    .WithDescription("Get a movie by its ID");

// Search movies by title
app.MapGet("/movies/search", (string? title, string? director, string? genre) =>
{
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

    return result.ToList();
})
    .WithName("SearchMovies")
    .WithDescription("Search movies by title, director, or genre");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
