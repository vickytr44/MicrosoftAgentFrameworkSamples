using Agent.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace SimpleAgent.Agents;

public static class HumanInTheLoop
{
    public static async Task Run(IChatClient chatClient)
    {
        AIFunction weatherFunction = AIFunctionFactory.Create(GetWeather);
        AIFunction approvalRequiredWeatherFunction =
            new ApprovalRequiredAIFunction(weatherFunction);

        var agent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful assistant.",
            name: "HumanInTheLoopChatAgent",
            tools: [approvalRequiredWeatherFunction]
        );

        await ChatHelper.ChatLoop(agent);
    }


    [Description("Get the weather for a given location.")]
    static string GetWeather(
        [Description("The location to get the weather for.")]
        string location)
        => $"The weather in {location} is cloudy with a high of 15°C.";
}

