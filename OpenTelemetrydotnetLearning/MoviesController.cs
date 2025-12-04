using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenTelemetrydotnetLearning
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(ILogger<MoviesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetMovies()
        {
            _logger.LogInformation("API request received for getting movies from controller");
            
            var movies = new[]
            {
                new { Id = 1, Title = "Inception", Director = "Christopher Nolan" },
                new { Id = 2, Title = "The Matrix", Director = "The Wachowskis" },
                new { Id = 3, Title = "Interstellar", Director = "Christopher Nolan" }
            };

            _logger.LogInformation("Returning {MovieCount} movies from controller", movies.Length);
            return Ok(movies);
        }
    }
}
