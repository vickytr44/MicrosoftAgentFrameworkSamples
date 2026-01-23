using Agent.Shared;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();

WebApplication app = builder.Build();

// Create the AI agent
var chatClient = Client.AzureChatClient("AGUIAgent");

var agent = chatClient.AsAIAgent(
    name: "AGUIAssistant",
    instructions: "You are a helpful assistant.");

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

await app.RunAsync();