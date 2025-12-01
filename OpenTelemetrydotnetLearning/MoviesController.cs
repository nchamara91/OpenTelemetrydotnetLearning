using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OpenTelemetrydotnetLearning
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetMovies()
        {
            var movies = new[]
            {
                new { Id = 1, Title = "Inception", Director = "Christopher Nolan" },
                new { Id = 2, Title = "The Matrix", Director = "The Wachowskis" },
                new { Id = 3, Title = "Interstellar", Director = "Christopher Nolan" }
            };

            return Ok(movies);
        }
    }
}
