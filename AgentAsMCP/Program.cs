using AgentAsMCP;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseKestrelHttpsConfiguration();
builder.WebHost.UseUrls("https://localhost:5001");


var chatClient = Client.AzureChatClient("MCPAgent");

var agent = new ChatClientAgent(
           chatClient,
           instructions: "You are a helpful assistant that provides concise and informative responses.",
           name: "TestAgent"
       );

McpServerTool tool = McpServerTool.Create(agent.AsAIFunction());

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools([tool]);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();


app.MapMcp("/mcp");
app.MapGet("/health", () => "OK");


app.Run();


[Description("Get the weather for a given location.")]
static string GetWeather(
    [Description("The location to get the weather for.")]
        string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";