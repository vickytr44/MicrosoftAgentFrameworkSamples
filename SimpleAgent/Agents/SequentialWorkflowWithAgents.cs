using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace SimpleAgent.Agents;

public static class SequentialWorkflowWithAgents
{
    public static async Task Run(IChatClient chatClient)
    {
        var frenchAgent = new ChatClientAgent(chatClient, instructions: "Translate the input text to french" , name: "FrenchTranslatorAgent");

        var spanishAgent = new ChatClientAgent(chatClient, instructions: "Translate the input text to spanish" , name: "SpanishTranslatorAgent");

        var workflow = new WorkflowBuilder(frenchAgent)
            .AddEdge(frenchAgent, spanishAgent).WithOutputFrom(spanishAgent)
            .Build();

        // Execute the workflow
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, "Hello World!"));

        // Must send the turn token to trigger the agents.
        // The agents are wrapped as executors. When they receive messages,
        // they will cache the messages and only start processing when they receive a TurnToken.
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
            if (evt is AgentRunUpdateEvent executorComplete)
            {
                Console.WriteLine($"{executorComplete.Data}");
            }
        }
    }
}
