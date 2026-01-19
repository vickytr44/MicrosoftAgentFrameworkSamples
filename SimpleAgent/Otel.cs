using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Azure.Monitor.OpenTelemetry.Exporter;

namespace SimpleAgent;

public static class Otel
{
    public static void Configure(string sourceName, out TracerProvider? tracerProvider, out MeterProvider? meterProvider, out ILoggerFactory loggerFactory)
    {
        var SourceName = sourceName;

        var resourceBuilder = ResourceBuilder
        .CreateDefault()
        .AddService(SourceName);

        var applicationInsightsConnectionString = "";

        //var otlpEndpoint = "http://localhost:4317";

        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource(SourceName)
            .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry
            .AddSource("*Microsoft.Extensions.Agents*") // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry
            .AddHttpClientInstrumentation()
            //.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint))
            .AddAzureMonitorTraceExporter(options => options.ConnectionString = applicationInsightsConnectionString)
            //.AddConsoleExporter()
            .Build();

        meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("*Microsoft.Agents.AI") // Agent Framework metrics
            .AddHttpClientInstrumentation() // HTTP client metrics
            .AddRuntimeInstrumentation() // Runtime metrics
            .AddAzureMonitorMetricExporter(options => options.ConnectionString = applicationInsightsConnectionString)
            //.AddConsoleExporter()
            .Build();

        loggerFactory = LoggerFactory.Create(builder =>
        {
            // Add OpenTelemetry as a logging provider
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddAzureMonitorLogExporter(options => options.ConnectionString = applicationInsightsConnectionString);
                //options.AddConsoleExporter();
                // Format log messages. This is default to false.
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            })
            .SetMinimumLevel(LogLevel.Debug);
        });

        // Create a logger instance for your application
        var logger = loggerFactory.CreateLogger<Program>();
    }
}
