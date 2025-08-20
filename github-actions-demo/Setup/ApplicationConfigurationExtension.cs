using github_actions_demo.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace github_actions_demo.Setup;
/// <summary>
/// Application configuration extension
/// </summary>
public static class ApplicationConfigurationExtension
{
    /// <summary>
    /// Uses the services.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="environment">The environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    public static WebApplication UseServices(this WebApplication app,
           IHostEnvironment environment,
           IConfiguration configuration)
    {
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            IdentityModelEventSource.ShowPII = true;
        }
        else
        {
            app.UseHsts();
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseSecurityHeadersConfigurations();

        app.UseHttpsRedirection();

        if (!environment.IsDevelopment())
        {
            //response compression BEFORE
            //static files or static files will return
            //them without being compressed
            //sometimes inteferes with hot reload on local dev
            app.UseResponseCompression();
        }
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        app.UseCookiePolicy();
        app.UseSerilogRequestLogging();
        app.UseStatusCodePages();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();
        app.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        }).AllowAnonymous();

        app.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
