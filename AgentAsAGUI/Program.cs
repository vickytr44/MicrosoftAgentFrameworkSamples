using Agent.Shared;
using AgentAsAGUI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Add(ProverbsAgentSerializerContext.Default));
builder.Services.AddAGUI();

WebApplication app = builder.Build();

// Create the AI agent
var chatClient = Client.AzureChatClient("AGUIAgent");

//var agent = chatClient.AsAIAgent(
//    name: "AGUIAssistant",
//    instructions: "You are a helpful assistant.",
//    tools: AgentTools.FunctionTools);

var agent = new ProverbsAgentFactory(
    chatClient,
    builder.Configuration,
    System.Text.Json.JsonSerializerOptions.Default).CreateProverbsAgent();

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

await app.RunAsync();

public partial class Program { }
