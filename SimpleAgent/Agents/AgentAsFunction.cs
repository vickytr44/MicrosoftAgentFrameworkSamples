using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SimpleAgent.Helpers;
using System.ComponentModel;

namespace SimpleAgent.Agents;

public class AgentAsFunction
{
    public static async Task Run(IChatClient chatClient)
    {
        AIFunction weatherFunction = AIFunctionFactory.Create(GetWeather);

        var weatherAgent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful assistant.",
            name: "HumanInTheLoopChatAgent",
            tools: [weatherFunction]
        );

        var agent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful assistant that only uses the tools when necessary.",
            name: "AgentAsFunctionChatAgent",
            tools: [weatherAgent.AsAIFunction()]
        );

        await ChatHelper.ChatLoop(agent);
    }

    [Description("Get the weather for a given location.")]
    static string GetWeather(
    [Description("The location to get the weather for.")]
        string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";
}
