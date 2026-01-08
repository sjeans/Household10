using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Household.Shared.Helpers;
using Household.Shared.Services;
using Household.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using StackExchange.Redis;

//namespace Household.Shared.Helpers;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        ////🧹        services.AddApiVersioning(options =>
        ////          {
        ////              options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        ////              options.AssumeDefaultVersionWhenUnspecified = true;
        ////              options.ReportApiVersions = true;
        ////              options.ApiVersionReader = new MediaTypeApiVersionReader("v");
        ////              options.ApiVersionReader = ApiVersionReader.Combine(
        ////                  new UrlSegmentApiVersionReader()
        ////              );
        ////          });

        string blazorKey = configuration.GetValue<string>("BlazorizeKey") ?? string.Empty;
        string[] allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
        int.TryParse(configuration.GetValue<string>("CorsPreflightSeconds"), out int preflightSeconds);
        if(preflightSeconds <= 0)
            preflightSeconds = 600; // 10 minutes

        AddLoggingSupport(services, configuration);
        AddBlazorSupport(services, blazorKey);
        AddCORSSupport(services, allowedOrigins, preflightSeconds);
        AddRedisSupport(services, configuration);
        AddAuthenticationAndAuthorization(services, configuration);

        services.AddScoped<IPageHistoryState, PageHistoryState>();
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IAppJsonDeserializer, AppJsonDeserializer>();
        services.AddScoped<ITvScheduleService, TvScheduleService>();
        services.AddScoped<IUserIpService, UserIpService>();

        AddHttpClientSupport(services, configuration);

        //services.AddScoped<IJwtUtils, JwtUtils>();
        //services.AddScoped<SignInManager<ApplicationUser>>();

        return services;
    }

    private static void AddAuthenticationAndAuthorization(IServiceCollection services, IConfiguration configuration)
    {
        // Authentication & Authorization
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = ".Household.Auth";
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            //options.Cookie.SameSite = SameSiteMode.Lax;
            //options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        }) // Required for sign-in persistence
        .AddOpenIdConnect(options =>
        {
            IConfigurationSection cfg = configuration.GetSection("Keycloak");
            options.Authority = cfg["Authority"];
            options.ClientId = cfg["ClientId"];
            options.ClientSecret = cfg["ClientSecret"];
            options.CallbackPath = cfg["CallbackPath"];

            options.NonceCookie.IsEssential = true;
            options.CorrelationCookie.IsEssential = true;

            //options.NonceCookie.SameSite = SameSiteMode.Lax;
            //options.NonceCookie.SecurePolicy = CookieSecurePolicy.None;

            //options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            //options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;

            options.NonceCookie.SameSite = SameSiteMode.None;
            options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

            options.CorrelationCookie.SameSite = SameSiteMode.None;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuers = [cfg["Authority"]],
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "name", // This is what populates @context.User.Identity?.Name
                RoleClaimType = "role",
            };

            options.MetadataAddress = string.Concat(cfg["Authority"], "/.well-known/openid-configuration");
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.RequireHttpsMetadata = false;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.MapInboundClaims = true;

            options.SignedOutCallbackPath = cfg["SignoutCallbackPath"];

            options.Events = new OpenIdConnectEvents()
            {
                // Event handlers for more control (optional)
                OnTicketReceived = ticketReceivedContext =>
                {
                    Log.Logger.Information("Started processing user roles.");
                    List<AuthenticationToken> tokens = [.. ticketReceivedContext.Properties!.GetTokens()];
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)ticketReceivedContext.Principal!.Identity!;
                    string access_token = tokens.First(token => token.Name == "access_token")?.Value ?? string.Empty;

                    JwtSecurityTokenHandler handler = new();
                    JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(access_token);
                    string issuer = jwtSecurityToken.Payload.First(x => x.Key.Equals("iss")).Value.ToString() ?? string.Empty;

                    JObject obj = JObject.Parse(jwtSecurityToken.Claims.First(c => c.Type == "realm_access").Value);
                    List<JToken> roleAccess = obj.GetValue("roles")?.ToList() ?? [];
                    foreach (JToken role in roleAccess!)
                    {
                        claimsIdentity.AddClaim(new Claim("role", role.ToString(), "http://www.w3.org/2001/XMLSchema#string", issuer, issuer, null));
                    }
                    if (claimsIdentity.FindFirst(x => x.Type.Equals("role") && x.Value.Equals("Supervisor")) is not null)
                    {
                        obj = JObject.Parse(jwtSecurityToken.Claims.First(c => c.Type == "resource_access").Value);
                        List<JToken> realmAccess = obj.GetValue("realm-management")?.ToList() ?? [];
                        roleAccess = ((JContainer)realmAccess[0]).First?.ToList() ?? [];
                        foreach (JToken role in roleAccess!)
                        {
                            claimsIdentity.AddClaim(new Claim("role", role.ToString(), "http://www.w3.org/2001/XMLSchema#string", issuer, issuer, null));
                        }
                    }

                    //Log.Logger.Information("User '{User}' logged in with roles: {Roles}", claimsIdentity.Name,
                    //    string.Join(", ", claimsIdentity.FindAll("role").Select(r => r.Value)));
                    Log.Logger.Information("Finished processing user roles.");

                    return Task.CompletedTask;
                },
                
                OnAuthenticationFailed = authenticationFailedContext =>
                {
                    Log.Logger.Error(string.Concat("JWT validation failed: ", authenticationFailedContext.Exception));

                    // Capture the error message from the Identity Provider (IdP)
                    string errorCode = authenticationFailedContext.Exception?.Message ?? "unknown_error";
                    string errorDesc = authenticationFailedContext.Exception?.GetInnerMessage() ?? "An error occurred during remote authentication.";

                    authenticationFailedContext.Response.Redirect($"/auth-error?code={errorCode}&description={Uri.EscapeDataString(errorDesc)}");
                    authenticationFailedContext.HandleResponse();
                    return Task.FromResult(0);
                },

                OnRemoteFailure = remoteFailureContext =>
                {
                    // Capture the error message from the Identity Provider (IdP)
                    string errorCode = remoteFailureContext.Failure?.Message ?? "unknown_error";
                    string errorDesc = remoteFailureContext.Failure?.GetInnerMessage() ?? "An error occurred during remote authentication.";

                    Log.Logger.Error(string.Concat("Remote failure: ", errorCode));
                    remoteFailureContext.Response.Redirect($"/auth-error?code={errorCode}&description={Uri.EscapeDataString(errorDesc)}");
                    remoteFailureContext.HandleResponse();
                    return Task.FromResult(0);
                },

                OnAccessDenied = accessDeniedContext =>
                {
                    string username = accessDeniedContext.HttpContext.User.Identity?.Name ?? "unknown user";

                    Log.Logger.Warning("OIDC Access Denied for user: {name}.", username);
                    return Task.CompletedTask;
                },

                OnRedirectToIdentityProvider = (RedirectContext redirectContext) =>
                {
                    // 1. Gather context for the log
                    string loginHint = redirectContext.ProtocolMessage.LoginHint ?? "none";
                    string redirectUri = redirectContext.ProtocolMessage.RedirectUri;
                    string issuer = redirectContext.ProtocolMessage.IssuerAddress;

                    // 2. High-priority log for auditing and debugging
                    Log.Logger.Information("OIDC Redirect: Initiating login for user hint {LoginHint}. Redirecting to {Issuer} with Callback {RedirectUri}",
                        loginHint, issuer, redirectUri);

                    // Optional: Dynamically fix Redirect URIs (common in load-balanced/Docker envs)
                    if (redirectContext.Request.Headers.ContainsKey("X-Forwarded-Proto"))
                    {
                        //RedirectContext.ProtocolMessage.RedirectUri = RedirectContext.ProtocolMessage.RedirectUri.Replace("http://", "https://");
                        Log.Logger.Warning("Redirecting to: {redirect}", redirectContext.ProtocolMessage.RedirectUri);
                    }

                    return Task.CompletedTask;
                },

                OnMessageReceived = messageReceivedContext =>
                {
                    // High Priority: Log every incoming auth attempt for security forensics
                    if (!string.IsNullOrEmpty(messageReceivedContext.ProtocolMessage.Error))
                    {
                        Log.Logger.Error("OIDC Message Error: {Error} - {Description}",
                            messageReceivedContext.ProtocolMessage.Error,
                            messageReceivedContext.ProtocolMessage.ErrorDescription);
                    }
                    else
                    {
                        Log.Logger.Information("OIDC Message Received for State: {State}",
                            messageReceivedContext.ProtocolMessage.State);
                    }
                    return Task.CompletedTask;
                },

                OnUserInformationReceived = context =>
                {
                    // High Priority: Log successes/failures in profile enrichment
                    JsonDocument userPayload = context.User; // This is the JSON payload from the IdP

                    if (userPayload.RootElement.ValueKind == JsonValueKind.Undefined)
                        Log.Logger.Warning("OIDC UserInfo endpoint returned an empty payload.");
                    else
                        Log.Logger.Information("Successfully retrieved extra claims for subject: {Sub}",
                            userPayload.RootElement.GetProperty("sub").GetString());

                        // Example: Logic to sync these claims to a local database could go here

                    return Task.CompletedTask;
                }
            };

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
        });
    }

    private static void AddBlazorSupport(IServiceCollection services, string blazorizeKey)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();   // required for hybrid hosting
        services.AddControllers();

        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        services.AddBlazorise(options =>
        {
            options.Immediate = true;
            options.ProductToken = blazorizeKey;
        })
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();

        //services.AddControllers().AddJsonOptions(jsonOptions =>
        //{
        //    jsonOptions.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        //    jsonOptions.JsonSerializerOptions.MaxDepth = 32;  // or however deep you need
        //});
    }

    private static void AddCORSSupport(IServiceCollection services, string[] allowedOrigins, int preflightseconds)
    {
        services.AddCors(opt =>
        {
            opt.AddPolicy(name: "CorsPolicy", builder =>
            {
                builder.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .WithMethods("PUT", "POST", "DELETE", "GET", "OPTIONS")
                    .SetPreflightMaxAge(TimeSpan.FromSeconds(preflightseconds)) // 24 hours
                    //.SetPreflightMaxAge(TimeSpan.FromMinutes(10)) // Development
                    .AllowCredentials();
            });
        });
    }

    private static void AddLoggingSupport(IServiceCollection services, IConfiguration configuration)
    {
        LoggingLevelSwitch levelSwitch = new()
        {
            MinimumLevel = LogEventLevel.Information
        };

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogEventLevel.Warning)
            .Enrich.FromLogContext()

            .Filter.ByExcluding(logEvent =>
            {
                if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContextValue) &&
                    sourceContextValue is ScalarValue scalarValue &&
                    scalarValue.Value is string sourceContext &&
                    sourceContext.Equals("LuckyPennySoftware.MediatR.License") &&
                    logEvent.Level <= LogEventLevel.Warning)
                {
                    return true; // Exclude this log event
                }
                return false; // Include this log event
            })

            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.IdentityModel", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging", LogEventLevel.Debug)

            .WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true,
            //.WriteTo.Console(theme: AnsiConsoleTheme.Literate, applyThemeToRedirectedOutput: true,
            //.WriteTo.Console(theme: SystemConsoleTheme.Literate, applyThemeToRedirectedOutput: true,
                             outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss:fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
        });

        //builder.Host.UseSerilog((context, services, configuration) => configuration
        //    .ReadFrom.Configuration(context.Configuration) // Read settings from appsettings.json
        //    .ReadFrom.Services(services)
        //    .Enrich.FromLogContext()
        //    .WriteTo.Console(theme: AnsiConsoleTheme.Code,
        //                             outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss:fff} [{Level:u}] {Message:lj}{NewLine}{Exception}"));

        services.AddSerilog(dispose: true);
        services.AddSingleton(Log.Logger);
    }

    private static void AddHttpClientSupport(IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddHttpContextAccessor();

        string? scheme = configuration.GetValue<string>("ServerScheme");
        string? host = configuration.GetValue<string>("ServerHost");
        int? port = configuration.GetValue<int>("ServerPort");

        string? k3s = Environment.GetEnvironmentVariable("MISC_SERVER");
        if (k3s is not null)
            host = k3s;

        Log.Information(string.Concat("k3s: ", k3s));

        scheme ??= "http";
        host ??= "appcontainer.home";
        port ??= 8282;

        UriBuilder uriBuilder = new(scheme, host, port.Value);

        string? serverUrl = configuration["ServerUrl"] ?? string.Empty;
        string? serverScheme = configuration["ServerScheme"] ?? string.Empty;
        string? serverHost = configuration["ServerHost"] ?? string.Empty;
        if (!int.TryParse(configuration["ServerPort"], out int serverPost))
            serverPost = 8282;

        uriBuilder = new(serverScheme, serverHost, serverPost);

        //services.AddSingleton<ITokenService, CachedTokenService>();
        services.AddSingleton<ITokenService, DistributedTokenService>();
        services.AddTransient<KeycloakAuthHandler>();

        services.AddHttpClient("ApiClient").ConfigureHttpClient((sp, client) =>
        {
            client.BaseAddress = uriBuilder.Uri;
        })
            .AddHttpMessageHandler<KeycloakAuthHandler>();
    }

    private static void AddRedisSupport(IServiceCollection services, IConfiguration configuration)
    {
        string? cs = configuration.GetConnectionString("Redis") ?? string.Empty;

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cs;
            options.InstanceName = "HouseholdCache_";
        });
        // 2. Raw Redis Connection (StackExchange.Redis)
        //services.AddSingleton<IConnectionMultiplexer>(sp =>
        //{
        //    return ConnectionMultiplexer.Connect(cs);
        //});
        services.AddSingleton(sp =>
        {
            try
            { 
                ConnectionMultiplexer mux = ConnectionMultiplexer.Connect(cs);
                List<RedLockMultiplexer> multiplexers = [mux];

                return RedLockFactory.Create(multiplexers);
            }
            catch (Exception ex)
            {
                Log.Error("Error creating RedLockFactory: " + ex.Message);
                return null!;
            }
        });

        // Generic cache service
        services.AddScoped(typeof(ICacheService<>), typeof(DistributedCacheService<>));
        services.AddScoped<ITokenService, DistributedTokenService>();
    }
}
