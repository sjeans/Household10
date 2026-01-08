namespace Household.Shared.Dtos;

public class AppSettings
{
    public Serilog Serilog { get; set; } = default!;
    public Authentication Authentication { get; set; } = default!;
    public string BlazorizeKey { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string ServerHost { get; set; } = string.Empty;
    public string ServerScheme { get; set; } = string.Empty;
    public string ServerPort { get; set; } = string.Empty;
    public string AllowedHosts { get; set; } = string.Empty;
}

public class Serilog
{
    public string[] Using { get; set; } = [];
    public Minimumlevel MinimumLevel { get; set; } = default!;
    public Writeto[] WriteTo { get; set; } = default!;
    public string[] Enrich { get; set; } = [];
}

public class Minimumlevel
{
    public string Default { get; set; } = string.Empty;
    public Override Override { get; set; } = default!;
}

public class Override
{
    public string System { get; set; } = string.Empty;
    public string Microsoft { get; set; } = string.Empty;
    public string MicrosoftHostingLifetime { get; set; } = string.Empty;
    public string MicrosoftAspNetCoreHostingDiagnostics { get; set; } = string.Empty;
    public string MicrosoftAspNetCoreHttpLoggingHttpLoggingMiddleware { get; set; } = string.Empty;
}

public class Writeto
{
    public string Name { get; set; } = string.Empty;
    public Args Args { get; set; } = default!;
}

public class Args
{
    public string theme { get; set; } = string.Empty;
    public string outputTemplate { get; set; } = string.Empty;
}

public class Authentication
{
    public Schemes Schemes { get; set; } = default!;
}

public class Schemes
{
    public Keycloak Keycloak { get; set; } = default!;
    public Keycloak KeycloakBackend { get; set; } = default!;
    public Keycloak KeycloakBackendTest { get; set; } = default!;
}

public class Keycloak
{
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
}
