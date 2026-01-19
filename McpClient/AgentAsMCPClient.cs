using Agent.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace McpClientSamples;

public static class AgentAsMCPClient
{
    public async static Task RunAsync()
    {
        var transport = new HttpClientTransport(new HttpClientTransportOptions() { Endpoint = new Uri("https://localhost:5001/mcp") });

        var mcpClient = await McpClient.CreateAsync(transport);

        var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        var chatClient = Client.AzureChatClient("MyAgentAsMCPClientApp");

        var agent = new ChatClientAgent(
           chatClient,
           instructions: "You are a helpful assistant that provides concise and informative responses.",
           name: "TestAgent",
           tools: [.. mcpTools.Cast<AITool>()]
       );

        await ChatHelper.ChatLoop(agent);
    }
}
