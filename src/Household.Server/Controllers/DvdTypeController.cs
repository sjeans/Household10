using Household.Shared.Models;
//using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class DvdTypeController : Controller
{
    private ILogger _logger;
    private HttpClient? _client;

    public DvdTypeController(ILogger<DvdTypeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _client = httpClientFactory.CreateClient("ApiClient");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDvdTypesAsync()
    {
        if (_client == null)
            return BadRequest("Cannot retrieve data for all dvd types");

        List<Dvdtype>? dvdTypes = new();

        try
        {
            _logger.LogInformation("Sending request to retrieve all dvd types");
            List<Dvdtype>? types = await _client.GetFromJsonAsync<List<Dvdtype>>("api/DiskType");
            _logger.LogInformation("Received response for all {count} dvd types", types?.Count);

            if (types != null)
            {
                types.ForEach(dvd =>
                {
                    dvdTypes = types;
                });
            }
            else
                return NotFound(dvdTypes);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all dvd types: {errMsg}", ex.Message);
            return BadRequest(ex.Message);
        }

        return Ok(dvdTypes);
    }
}
