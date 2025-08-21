using FluentValidation;
using github_actions_demo.Behaviors;
using github_actions_demo.Infrastructure.Contexts;
using github_actions_demo.Setup.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;

namespace github_actions_demo.Setup;
/// <summary>
/// Service collection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="environment">The environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddServices(
            this IServiceCollection services,
                IHostEnvironment environment,
                IConfiguration configuration,
                IHostBuilder host)
    {
        //add feature management
        //see example on how it is used
        //in the `HomeController` and `_Layout.cshtml` for accessing Privacy
        services.AddFeatureManagement();


        string? identityConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(identityConnectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not set in the configuration.");
        }
        //add db context for Asp.Identity
        //set up a local db for local development and
        //set up the connection string in `appsettings.Development.json`
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(identityConnectionString,
            sqloptions => sqloptions.EnableRetryOnFailure());
        });



        //add distributed memory cache
        services.AddDistributedMemoryCache();

        services.AddControllersWithViews();

        //add saml2 for sso with or without aspnet identity
        services.AddSaml2(configuration);
        //mediatr
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(Program).Assembly);
            config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        //fluent validation
        services.AddValidatorsFromAssembly(typeof(Program).Assembly,
            includeInternalTypes: true);


        //add automapper
        services.AddAutoMapper(typeof(Program).Assembly);

        //add open telemetry
        services.AddDiagnostics(configuration, environment, host);

        services.AddHttpContextAccessor();

        //add health checks
        services.AddHealthChecks()
            .AddCheck(
            name: "GitInfoHealthCheck",
            instance: new GitInfoHealthCheck(),
            tags: ["git"])

            .AddSqlServer(
                connectionString: identityConnectionString,
                healthQuery: "SELECT 1;",
                name: "SQL Database",
                failureStatus: HealthStatus.Degraded,
                tags: ["db", "sql", "sqlserver"])

        ;

        services.AddResponseCompressions();

        return services;
    }
}
