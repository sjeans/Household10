using Household.Shared.Dtos;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss:fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger(); // Use CreateBootstrapLogger for initial setup

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder();

    Log.Logger.Debug($"DETECTED ENVIRONMENT: {builder.Environment.EnvironmentName}");

    builder.Services.Configure<AppSettings>(builder.Configuration);
    builder.Services.Configure<Household.Shared.Dtos.Serilog>(builder.Configuration.GetSection("Serilog"));
    builder.Services.Configure<Authentication>(builder.Configuration.GetSection("Authentication"));

    // DI services
    builder.Services.AddServices(builder.Configuration);
    builder.Logging.AddFilter("LuckyPennySoftware.MediatR.License", LogLevel.None);

    // DI application services like pipeline behaviors, validators, etc.
    builder.Services.AddApplicationServices();

    string? trimmedContentRootPath = builder.Environment.ContentRootPath.TrimEnd(Path.DirectorySeparatorChar);

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"/mnt/Share/data-keys/household/"))
        .SetApplicationName(trimmedContentRootPath);

    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.MinimumSameSitePolicy = SameSiteMode.Unspecified; // Or SameSiteMode.None if over HTTPS
        options.Secure = CookieSecurePolicy.SameAsRequest;
    });

    builder.WebHost.UseStaticWebAssets();

    WebApplication app = builder.Build();

    Log.Information("Environment: {env}, Is Development: {dev}", app.Environment.EnvironmentName, app.Environment.IsDevelopment());
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        //app.UseHsts();
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    app.UseCookiePolicy();
    app.UseStaticFiles();

    // 5. Add Serilog request logging middleware
    // Place this before other middleware that might generate log events you want to capture
    // like UseRouting, UseAuthentication, UseAuthorization
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseCors("CorsPolicy");
    app.UseAntiforgery();

    app.UseAuthorization();
    app.UseAuthentication();

    //// Add a custom middleware to enrich the log context with user information AFTER authentication
    //app.Use(async (context, next) =>
    //{
    //    ClaimsPrincipal user = context.User;
    //    if (user?.Identity?.IsAuthenticated == true)
    //    {
    //        // Enrich the log context with user details, e.g., username, user ID
    //        Serilog.Context.LogContext.PushProperty("UserName", user.Identity.Name);
    //        Serilog.Context.LogContext.PushProperty("UserId", user.FindFirst("sub")?.Value); // Use the appropriate claim type
    //    }
    //    await next();
    //});

    app.MapStaticAssets();

    app.MapRazorComponents<Household.Server.Components.App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode();

    app.MapFallbackToPage("/Index");
    app.MapControllers(); // if using controllers

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
