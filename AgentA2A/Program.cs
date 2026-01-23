using A2A;
using A2A.AspNetCore;
using Agent.Shared;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var chatClient = Client.AzureChatClient("AgentA2AApp");

builder.Services.AddSingleton(chatClient);

AIFunction weatherFunction = AIFunctionFactory.Create(GetWeather);
// HITL is not supported by A2A protocol.
AIFunction approvalRequiredWeatherFunction =
    new ApprovalRequiredAIFunction(weatherFunction);

var agent = builder.AddAIAgent(
    "AgentA2A",
    (sp, agentName) =>
    {
        var chatClient = sp.GetRequiredService<IChatClient>();

        return new ChatClientAgent(
            name: agentName,
            instructions: "You are a helpful assistant that provides concise and informative responses.",
            chatClient: chatClient,
            tools: [weatherFunction]
        );
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var agentSkill = new AgentSkill()
{
    Name = "GetWeather",
    Description = "Get the weather for a given location."
};

var agentcard = new AgentCard()
{
    Name = "AgentA2A",
    Description = "An agent exposed via A2A protocol",
    Version = "1.0",
    Skills = [agentSkill]
};

// Expose the agent via A2A protocol. You can also customize the agentCard
app.MapA2A(agent, path: "/a2a/agent", agentCard: agentcard);

app.Run();

[Description("Get the weather for a given location.")]
static string GetWeather(
    [Description("The location to get the weather for.")]
        string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";
