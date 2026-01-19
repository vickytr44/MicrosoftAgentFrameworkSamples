using Agent.Shared;
using Microsoft.Agents.AI;
using ModelContextProtocol.Server;

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