using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Agent.Shared;

public class ChatHelper
{
    public static async Task ChatLoop(ChatClientAgent agent)
    {
        AgentThread thread = agent.GetNewThread();

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
            AgentRunResponse response =
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

    private static void PrintAssistantMessages(AgentRunResponse response)
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
