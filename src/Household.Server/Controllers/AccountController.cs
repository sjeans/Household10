using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Household.Server.Controllers;

[Produces("application/json")]
[ApiController]
//[Route("[controller]/[action]")]
[Route("Account")]
public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private IConfiguration _config;

    public AccountController(IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        _httpClientFactory = clientFactory;
        _config = configuration;
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("Logout")]
    public async Task<IActionResult> Logout(string? returnUrl = "/")
    {
        // Retrieve the id_token from the authentication properties.
        // This token was stored during the initial login.
        string? idToken = await HttpContext.GetTokenAsync("id_token");

        // Prepare the OpenID Connect logout options.
        AuthenticationProperties authenticationProperties = new();
        if (!string.IsNullOrEmpty(idToken))
        {
            // Add the id_token_hint to the logout request.
            authenticationProperties.SetParameter("id_token_hint", idToken);
        }

        // Sign out from the application and initiate the OIDC logout.
        // The SignOut method will automatically construct the logout URL 
        // and include the id_token_hint if provided in the properties.
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authenticationProperties);
        await HttpContext.SignOutAsync("Cookies"); // Also sign out from local cookies
        return Redirect(returnUrl);

        //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //return SignOut(new AuthenticationProperties { RedirectUri = "/" },
        //    OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("Token")]
    public async Task<IActionResult> GetToken()
    {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        string? refreshToken = await HttpContext.GetTokenAsync("refresh_token");
        string? idToken = await HttpContext.GetTokenAsync("id_token");

        return Ok(new
        {
            accessToken,
            refreshToken,
            idToken
        });
    }

    [HttpPost("RegisterUser")]
    public async Task<IActionResult> RegisterUser([FromBody] Root request)
    {
        HttpClient client = _httpClientFactory.CreateClient("ApiClient");
        string authority = _config["Authentication:Schemes:KeycloakBackend:Authority"] ?? string.Empty;
        string realm = authority.Substring(authority.LastIndexOf("/") + 1);
        HttpResponseMessage response = await client.PostAsJsonAsync($"/admin/realms/{realm}/users", new
        {
            username = request.username,
            email = request.email,
            enabled = Convert.ToBoolean(request.enabled),
            firstName = request.firstName,
            lastName = request.lastName,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = request.password
                }
            }
        });

        if(!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        /*POST /admin/realms/{realm}/users
        {
          "username": "john.doe",
          "email": "john.doe@example.com",
          "enabled": true,
          "firstName": "John",
          "lastName": "Doe",
          "credentials": [{
            "type": "password",
            "value": "password"
          }]
        }
        */

        return Ok();
    }
    // GET /admin/realms/{realm}/users/count
    // GET /admin/realms/{realm}/users
    // GET /admin/realms/{realm}/users/{user-id}
    // POST /admin/realms/{realm}/users
    // PUT /admin/realms/{realm}/users/{user-id}
    // POST /admin/realms/{realm}/users/{user-id}/impersonation

    public class Root
    {
        public string username { get; set; }
        public string email { get; set; }
        public string enabled { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string password { get; set; }
    }
}