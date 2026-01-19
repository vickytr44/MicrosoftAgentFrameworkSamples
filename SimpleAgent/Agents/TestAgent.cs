using Agent.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace SimpleAgent.Agents;

public static class TestAgent
{
    public static async Task Run(IChatClient chatClient)
    {
        var agent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful assistant that provides concise and informative responses.",
            name: "TestAgent"
        );

        await ChatHelper.ChatLoop(agent);
    }
}
