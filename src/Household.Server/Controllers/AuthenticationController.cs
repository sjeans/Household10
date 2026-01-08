using System.Web;
using Household.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Household.Server.Controllers;

//[EnableCors("CorsPolicy")]
[Produces("application/json")]
[ApiController]
[Route("/api/[controller]")]
public class AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController > logger, IHttpClientFactory httpClientFactory) : Controller
{

    protected readonly IConfiguration _config = configuration;
    private ILogger<AuthenticationController> _logger = logger;
    private HttpClient _httpClient = httpClientFactory.CreateClient();

    [HttpGet("login")]
    public IActionResult Login(string redirect = "/signin-oidc")
    {

        // Get Keycloak configuration from appsettings.json or similar
        var keycloakAuthority = _config["Authentication:Schemes:KeycloakStandardFlow:Authority"];
        var keycloakClientId = _config["Authentication:Schemes:KeycloakStandardFlow:ClientId"];
        var redirectUri = redirect; // signin-oidc"; // _config["Authentication:Schemes:Keycloak:RedirectUri"]; // Where Keycloak should redirect after login

        // Construct the Keycloak authorization URL
        string authorizationUrl = $"{keycloakAuthority}/protocol/openid-connect/auth" +
                               $"?client_id={keycloakClientId}" +
                               $"&redirect_uri={HttpUtility.UrlEncode(redirectUri)}" +
                               "&response_type=code" + // Or "token id_token" for Implicit Flow
                               "&scope=openid profile email";

        // Redirect the user to the Keycloak login page
        return Redirect(authorizationUrl);

        //try
        //{
        //    _logger.LogInformation("Calling api/user");
        //    List<User>? usersList = await _httpClient.GetFromJsonAsync<List<User>>("api/User/");
        //    _logger.LogInformation("Response for users returned {0}", usersList?.Count);

        //    if (usersList != null)
        //        return Ok(usersList);
        //    else
        //        return NotFound(usersList);

        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Failure retrieving users: {0}", ex.Message);
        //    return BadRequest(ex.Message);
        //}
        //var authenticationProperties = new AuthenticationProperties
        //{
        //    RedirectUri = "/"
        //};

        //return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("logout")]
    public async Task Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }
}
