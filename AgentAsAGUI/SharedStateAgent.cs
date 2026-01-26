using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentAsAGUI;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by ProverbsAgentFactory")]
internal sealed class SharedStateAgent : DelegatingAIAgent
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SharedStateAgent(AIAgent innerAgent, JsonSerializerOptions jsonSerializerOptions)
        : base(innerAgent)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
    {
        return RunStreamingAsync(messages, thread, options, cancellationToken).ToAgentResponseAsync(cancellationToken);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (options is not ChatClientAgentRunOptions { ChatOptions.AdditionalProperties: { } properties } chatRunOptions ||
            !properties.TryGetValue("ag_ui_state", out JsonElement state))
        {
            await foreach (var update in InnerAgent.RunStreamingAsync(messages, thread, options, cancellationToken).ConfigureAwait(false))
            {
                yield return update;
            }
            yield break;
        }

        var firstRunOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = chatRunOptions.ChatOptions.Clone(),
            AllowBackgroundResponses = chatRunOptions.AllowBackgroundResponses,
            ContinuationToken = chatRunOptions.ContinuationToken,
            ChatClientFactory = chatRunOptions.ChatClientFactory,
        };

        // Configure JSON schema response format for structured state output
        firstRunOptions.ChatOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema<ProverbsStateSnapshot>(
            schemaName: "ProverbsStateSnapshot",
            schemaDescription: "A response containing the current list of proverbs");

        ChatMessage stateUpdateMessage = new(
            ChatRole.System,
            [
                new TextContent("Here is the current state in JSON format:"),
                new TextContent(state.GetRawText()),
                new TextContent("The new state is:")
            ]);

        var firstRunMessages = messages.Append(stateUpdateMessage);

        var allUpdates = new List<AgentResponseUpdate>();
        await foreach (var update in InnerAgent.RunStreamingAsync(firstRunMessages, thread, firstRunOptions, cancellationToken).ConfigureAwait(false))
        {
            allUpdates.Add(update);

            // Yield all non-text updates (tool calls, etc.)
            bool hasNonTextContent = update.Contents.Any(c => c is not TextContent);
            if (hasNonTextContent)
            {
                yield return update;
            }
        }

        var response = allUpdates.ToAgentResponse();

        if (response.TryDeserialize(_jsonSerializerOptions, out JsonElement stateSnapshot))
        {
            byte[] stateBytes = JsonSerializer.SerializeToUtf8Bytes(
                stateSnapshot,
                _jsonSerializerOptions.GetTypeInfo(typeof(JsonElement)));
            yield return new AgentResponseUpdate
            {
                Contents = [new DataContent(stateBytes, "application/json")]
            };
        }
        else
        {
            yield break;
        }

        // ✅ NEW: detect whether the state actually changed
        bool stateChanged = ProverbsChanged(state, stateSnapshot);

        // ✅ Only narrate if something changed

        var summaryMessage = new ChatMessage(
                ChatRole.System,
                [new TextContent("Please provide a concise summary about the latest change in at most two sentences.")]);

        var secondRunMessages = messages.Concat(response.Messages);

        if (stateChanged)
            secondRunMessages = secondRunMessages.Append(summaryMessage);

        await foreach (var update in InnerAgent.RunStreamingAsync(secondRunMessages, thread, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private static bool ProverbsChanged(JsonElement oldState, JsonElement newState)
    {
        if (!oldState.TryGetProperty("proverbs", out var oldProverbs) ||
            !newState.TryGetProperty("proverbs", out var newProverbs))
        {
            // If property missing on either side, treat as changed
            return true;
        }

        // Must both be arrays
        if (oldProverbs.ValueKind != JsonValueKind.Array ||
            newProverbs.ValueKind != JsonValueKind.Array)
        {
            return true;
        }

        // Compare array length
        if (oldProverbs.GetArrayLength() != newProverbs.GetArrayLength())
        {
            return true;
        }

        // Compare elements one by one
        var oldEnum = oldProverbs.EnumerateArray();
        var newEnum = newProverbs.EnumerateArray();

        using var oldIt = oldEnum.GetEnumerator();
        using var newIt = newEnum.GetEnumerator();

        while (oldIt.MoveNext() && newIt.MoveNext())
        {
            // Assuming proverbs are strings
            if (!string.Equals(
                    oldIt.Current.GetString(),
                    newIt.Current.GetString(),
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false; // No change
    }

}
