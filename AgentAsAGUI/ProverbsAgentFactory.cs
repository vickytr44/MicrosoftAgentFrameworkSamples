using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;

namespace AgentAsAGUI;

public class ProverbsState
{
    public List<string> Proverbs { get; set; } = [];
}

public class ProverbsAgentFactory(IChatClient chatClient, IConfiguration configuration, System.Text.Json.JsonSerializerOptions jsonSerializerOptions)
{
    private readonly ProverbsState _state = new();

    public AIAgent CreateProverbsAgent()
    {
        var chatClientAgent = new ChatClientAgent(
            chatClient,
            name: "AGUIAgent",
            description: @"A helpful assistant",
            tools: [
                AIFunctionFactory.Create(GetProverbs, options: new() { Name = "get_proverbs", SerializerOptions = jsonSerializerOptions }),
                AIFunctionFactory.Create(AddProverbs, options: new() { Name = "add_proverbs", SerializerOptions = jsonSerializerOptions }),
                AIFunctionFactory.Create(SetProverbs, options: new() { Name = "set_proverbs", SerializerOptions = jsonSerializerOptions }),
                ..AgentTools.FunctionTools
            ]);

        return new SharedStateAgent(chatClientAgent, jsonSerializerOptions);
    }

    // =================
    // Tools
    // =================

    [Description("Get the current list of proverbs.")]
    private List<string> GetProverbs()
    {
        Console.WriteLine($"📖 Getting proverbs: {string.Join(", ", _state.Proverbs)}");
        return _state.Proverbs;
    }

    [Description("Add new proverbs to the list.")]
    private void AddProverbs([Description("The proverbs to add")] List<string> proverbs)
    {
        Console.WriteLine($"➕ Adding proverbs: {string.Join(", ", proverbs)}");
        _state.Proverbs.AddRange(proverbs);
    }

    [Description("Replace the entire list of proverbs.")]
    private void SetProverbs([Description("The new list of proverbs")] List<string> proverbs)
    {
        Console.WriteLine($"📝 Setting proverbs: {string.Join(", ", proverbs)}");
        _state.Proverbs = [.. proverbs];
    }
}
