using System.Runtime.InteropServices;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace github_actions_demo.Setup;
public static class DiagnosticsConfigurationExtensions
{
    public const string ActivitySourceName = "github_actions_demo-Service";

    public static IServiceCollection AddDiagnostics(
       this IServiceCollection services,
       IConfiguration configuration,
       IHostEnvironment environment,
        IHostBuilder host)
    {
        bool isOtelEnabled = configuration.GetValue<bool>("App:OpenTelemetry:Enable");
        bool isOtlpEnabled = configuration.GetValue<bool>("App:OpenTelemetry:Otlp:Enable");
        string? otlpEndpoint = configuration["App:OpenTelemetry:Otlp:EndpointUrl"];

        //change the service name and deployment environment as needed
        string otlpServiceName = $"{environment.EnvironmentName}.github_actions_demo";
        string oltpDeploymentEnvironment = $"{environment.EnvironmentName}";

        var apiKey = configuration.GetSection("App:OpenTelemetry:Otlp:EndpointUrlHeader")
            .GetChildren()
            .ToList();

        var resourceAttributes = new Dictionary<string, object>
        {
            ["host.name"] = Environment.MachineName,
            ["os.description"] = RuntimeInformation.OSDescription,
            ["deployment.environment"] = environment.EnvironmentName.ToUpperInvariant(),
            ["service.name"] = otlpServiceName
        };

        ResourceBuilder resourceBuilder = ResourceBuilder
        .CreateDefault()
        .AddService(otlpServiceName, serviceVersion: "1.0.0")
        .AddAttributes(resourceAttributes)
        .AddTelemetrySdk();

        //logging    
        services.AddLogging((loggingBuilder) => loggingBuilder
         .ClearProviders()
         .AddOpenTelemetry(options =>
         {
             options.IncludeScopes = true;
             options.ParseStateValues = true;
             options.IncludeFormattedMessage = true;
             options.SetResourceBuilder(resourceBuilder);
             options.AddProcessor(new NewRelicLogProcessor());

             if (isOtlpEnabled &&
             !string.IsNullOrEmpty(otlpEndpoint) &&
             !string.IsNullOrEmpty(apiKey[0].Value))
             {
                 options.AddOtlpExporter(exporter =>
                 {
                     // This must be an HTTPS URI; gRPC will be used.
                     exporter.Endpoint = new Uri(otlpEndpoint);
                     exporter.Headers = $"{apiKey[0].Key}={apiKey[0].Value}";

                 });
             }
             else
             {
                 options.AddConsoleExporter();
             }
         }));

        host.UseSerilog((context, provider, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
            configuration.WriteTo.OpenTelemetry(options =>
            {
                if (isOtlpEnabled &&
                !string.IsNullOrEmpty(otlpEndpoint) &&
                !string.IsNullOrEmpty(apiKey[0].Value))
                {
                    // This must be an HTTPS URI; gRPC will be used.
                    options.Endpoint = otlpEndpoint;
                    options.Headers = new Dictionary<string, string>()
                    {
                        { apiKey[0].Key, $"{apiKey[0].Value}" }
                    };
                }
                options.ResourceAttributes = resourceAttributes;

                options.IncludedData =
                        IncludedData.SpanIdField
                        | IncludedData.TraceIdField
                        | IncludedData.MessageTemplateTextAttribute
                        | IncludedData.MessageTemplateMD5HashAttribute;
            });
        }, writeToProviders: configuration.GetValue<bool>("App:OpenTelemetry:SerilogWriteToProviders"));

        if (isOtelEnabled)
        {
            //tracing and metrics
            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddSource(DiagnosticsConfigurationExtensions.ActivitySourceName)
                        .AddAspNetCoreInstrumentation();

                    if (isOtlpEnabled &&
                    !string.IsNullOrEmpty(otlpEndpoint) &&
                    !string.IsNullOrEmpty(apiKey[0].Value))
                    {
                        tracerProviderBuilder.AddOtlpExporter(exporter =>
                        {
                            if (!string.IsNullOrEmpty(otlpEndpoint) &&
                       !string.IsNullOrEmpty(apiKey[0].Value))
                            {
                                // This must be an HTTPS URI; gRPC will be used.
                                exporter.Endpoint = new Uri(otlpEndpoint);
                                exporter.Headers = $"{apiKey[0].Key}={apiKey[0].Value}";
                            }
                        });
                    }
                    else
                    {
                        tracerProviderBuilder.AddConsoleExporter();
                    }
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .SetResourceBuilder(resourceBuilder)
                         // Metrics provider from OpenTelemetry
                         .AddAspNetCoreInstrumentation()
                        // Metrics provides by ASP.NET Core in .NET 8
                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

                    if (isOtlpEnabled &&
                    !string.IsNullOrEmpty(otlpEndpoint) &&
                    !string.IsNullOrEmpty(apiKey[0].Value))
                    {
                        metrics.AddOtlpExporter(exporter =>
                        {
                            if (!string.IsNullOrEmpty(otlpEndpoint) &&
                       !string.IsNullOrEmpty(apiKey[0].Value))
                            {
                                // This must be an HTTPS URI; gRPC will be used.
                                exporter.Endpoint = new Uri(otlpEndpoint);
                                exporter.Headers = $"{apiKey[0].Key}={apiKey[0].Value}";
                            }
                        });
                    }
                    else
                    {
                        metrics.AddConsoleExporter();
                    }
                });
        }

        return services;
    }
}
