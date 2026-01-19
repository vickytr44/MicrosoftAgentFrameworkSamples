using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace SimpleAgent.Agents;

public class ConcurrentWorkflow
{
    public static async Task Run(IChatClient chatClient)
    {
        var physicAgent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful physics expert. You answer questions from physics perspective.",
            name: "PhysicsAgent"
        );

        var chemistryAgent = new ChatClientAgent(
            chatClient,
            instructions: "You are a helpful chemistry expert. You answer questions from chemistry perspective.",
            name: "ChemistryAgent"
        );

        var startExecutor = new ConcurrentStartExecutor();

        var aggregationExecutor = new ConcurrentAggregationExecutor();

        // Build the workflow by adding executors and connecting them
        var workflow = new WorkflowBuilder(startExecutor)
            .AddFanOutEdge(startExecutor, targets: [physicAgent, chemistryAgent])
            .AddFanInEdge(sources: [physicAgent, chemistryAgent], aggregationExecutor)
            .WithOutputFrom(aggregationExecutor)
            .Build();

        // Execute the workflow in streaming mode
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, "What is temperature?");
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is WorkflowOutputEvent output)
            {
                Console.WriteLine($"Workflow completed with results:\n{output.Data}");
            }
        }
    }
}

/// <summary>
/// Executor that starts the concurrent processing by sending messages to the agents.
/// </summary>
internal sealed class ConcurrentStartExecutor() : Executor<string>("ConcurrentStartExecutor")
{
    /// <summary>
    /// Starts the concurrent processing by sending messages to the agents.
    /// </summary>
    /// <param name="message">The user message to process</param>
    /// <param name="context">Workflow context for accessing workflow services and adding events</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public override async ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Broadcast the message to all connected agents. Receiving agents will queue
        // the message but will not start processing until they receive a turn token.
        await context.SendMessageAsync(new ChatMessage(ChatRole.User, message), cancellationToken);

        // Broadcast the turn token to kick off the agents.
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
    }
}

/// <summary>
/// Executor that aggregates the results from the concurrent agents.
/// </summary>
internal sealed class ConcurrentAggregationExecutor() :
    Executor<List<ChatMessage>>("ConcurrentAggregationExecutor")
{
    private readonly List<ChatMessage> _messages = [];

    /// <summary>
    /// Handles incoming messages from the agents and aggregates their responses.
    /// </summary>
    /// <param name="message">The message from the agent</param>
    /// <param name="context">Workflow context for accessing workflow services and adding events</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public override async ValueTask HandleAsync(List<ChatMessage> message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        this._messages.AddRange(message);

        if (this._messages.Count == 2)
        {
            var formattedMessages = string.Join(Environment.NewLine,
                this._messages.Select(m => $"{m.AuthorName}: {m.Text}"));
            await context.YieldOutputAsync(formattedMessages, cancellationToken);
        }
    }
}
