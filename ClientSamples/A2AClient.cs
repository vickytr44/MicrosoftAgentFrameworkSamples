using A2A;
using Agent.Shared;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.AI;
using System;
using System.Threading;

namespace ClientSamples;

public class A2AClient
{
    public async static Task Run()
    {
        var path = "/a2a/agent/v1/card";
        A2ACardResolver agentCardResolver = new(new Uri("http://localhost:5006"), agentCardPath: path);

        var agent = (await agentCardResolver.GetAIAgentAsync());

        await ChatLoop(agent);
    }

    private static async Task ChatLoop(AIAgent agent)
    {
        AgentThread thread = await agent.GetNewThreadAsync();

        Console.WriteLine("Type 'exit' to quit.\n");

        while (true)
        {
            Console.Write("> ");
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            // Send user message
            AgentResponse response =
                await agent.RunAsync(userInput, thread);

            // Check for approval-required function calls
            var approvalRequests = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionApprovalRequestContent>()
                .ToList();

            if (approvalRequests.Count != 0)
            {
                foreach (var request in approvalRequests)
                {
                    Console.WriteLine(
                        $"\n⚠️ Approval required to execute function: {request.FunctionCall.Name}");

                    Console.Write("Approve? (y/n): ");
                    var approval = Console.ReadLine();

                    bool approved = approval?.Equals("y", StringComparison.OrdinalIgnoreCase) == true;

                    var approvalMessage = new ChatMessage(
                        ChatRole.User,
                        [request.CreateResponse(approved)]
                    );

                    // Continue the same thread with approval response
                    var followUpResponse =
                        await agent.RunAsync(approvalMessage, thread);

                    PrintAssistantMessages(followUpResponse);
                }
            }
            else
            {
                PrintAssistantMessages(response);
            }
        }
    }

    private static void PrintAssistantMessages(AgentResponse response)
    {
        foreach (var message in response.Messages)
        {
            if (message.Role == ChatRole.Assistant)
            {
                foreach (var content in message.Contents)
                {
                    if (content is TextContent text)
                    {
                        Console.WriteLine($"\n🤖 {text.Text}\n");
                    }
                }
            }
        }
    }
}
