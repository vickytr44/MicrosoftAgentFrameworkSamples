using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SimpleAgent.Helpers;
using System.ComponentModel;
using System.Text.Json;

namespace SimpleAgent.Agents;

public class PersonInfo
{
    [Description("The person's full name.")]
    public string? Name { get; set; }

    [Description("The person's age.")]
    public int? Age { get; set; }

    [Description("The person's occupation.")]
    public string? Occupation { get; set; }

    [Description("Indicates if the person is married or not.")]
    public bool IsMarried { get; set; }
}

public class AgentWithStructuredOutput
{
    public static async Task Run(IChatClient chatClient)
    {
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(PersonInfo));

        ChatOptions chatOptions = new()
        {
            Instructions = "Extract the person's information and provide it in the specified JSON format.",
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
            schema: schema,
            schemaName: nameof(PersonInfo),
            schemaDescription: "Information about a person including their name, age, and occupation")
        };


        var chatclientOptions = new ChatClientAgentOptions()
        {
            Name = "PersonInfoAgent",
            ChatOptions = chatOptions
        };

        var agent = new ChatClientAgent(chatClient, chatclientOptions);

        await ChatHelper.ChatLoop(agent);
    }
}
