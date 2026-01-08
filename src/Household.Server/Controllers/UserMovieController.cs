using Household.Shared.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class UserMovieController : Controller
{
    private ILogger<UserMovieController> _logger;
    private HttpClient? _client;

    public UserMovieController(ILogger<UserMovieController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _client = httpClientFactory.CreateClient("ApiClient");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserTypesAsync()
    {
        if (_client == null)
            return BadRequest("Cannot retrieve data!");

        try
        {
            _logger.LogInformation("Retrieving all user movies");
            List<UserMovie>? userMovies = await _client.GetFromJsonAsync<List<UserMovie>>("api/UserMovie/");
            _logger.LogInformation("Response returned {count} user movies", userMovies?.Count);

            if (userMovies != null)
                return Ok(userMovies);
            else
                return NotFound(userMovies);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure returning user movies: {errMsg}", ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
