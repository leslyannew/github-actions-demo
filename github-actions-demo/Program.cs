using github_actions_demo.Setup;

using Serilog;

string? environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

Console.Title = $"{environmentName}.github_actions_demo";

#pragma warning disable CA1305
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();
#pragma warning restore CA1305

Log.Information("Starting up logging");

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    IServiceCollection services = builder.Services;

    services.AddServices(builder.Environment, builder.Configuration, builder.Host);

    WebApplication app = builder.Build();

    app.UseServices(builder.Environment, builder.Configuration);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException) //https://github.com/dotnet/efcore/issues/28478
{
    Log.Fatal(ex, "Unhandled Exception");
}

finally
{
    Log.Information("Log Complete");
    await Log.CloseAndFlushAsync();
}

public partial class Program { }
