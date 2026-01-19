using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SimpleAgent;

public static class Otel
{
    public static void Configure(string sourceName, out TracerProvider? tracerProvider, out MeterProvider? meterProvider, out ILoggerFactory loggerFactory)
    {
        var SourceName = sourceName;

        var resourceBuilder = ResourceBuilder
        .CreateDefault()
        .AddService(SourceName);

        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource(SourceName)
            .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry
            .AddSource("*Microsoft.Extensions.Agents*") // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry
            .AddConsoleExporter()
            .Build();

        meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("*Microsoft.Agents.AI") // Agent Framework metrics
            .AddConsoleExporter()
            .Build();

        loggerFactory = LoggerFactory.Create(builder =>
        {
            // Add OpenTelemetry as a logging provider
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.AddConsoleExporter();
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
