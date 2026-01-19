using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace SimpleAgent.Agents;

public static class OtelTest
{
    public static void Run()
    {
        var applicationInsightsConnectionString = ""; // Your Application Insights connection string


        var resource = ResourceBuilder.CreateDefault()
            .AddService("MyConsoleApp");

        using var tracerProvider =
            Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resource)
                .AddSource("My.Console")
                .AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString = applicationInsightsConnectionString;
                })
                .Build();

        var source = new ActivitySource("My.Console");

        using (var activity = source.StartActivity("Console.Main"))
        {
            activity?.SetTag("operation", "test");
            Thread.Sleep(500);
        }

        Console.WriteLine("Done. Waiting for flush...");
        Thread.Sleep(3000);
    }
}
