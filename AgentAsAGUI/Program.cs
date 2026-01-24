using Agent.Shared;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using System.ComponentModel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();

WebApplication app = builder.Build();

// Create the AI agent
var chatClient = Client.AzureChatClient("AGUIAgent");

AIFunction weatherFunction = AIFunctionFactory.Create(GetWeather);
AIFunction deleteEntityWithIdFunction = AIFunctionFactory.Create(DeleteEntityWithId);
//AIFunction approvalRequiredWeatherFunction =
//    new ApprovalRequiredAIFunction(weatherFunction);

var agent = chatClient.AsAIAgent(
    name: "AGUIAssistant",
    instructions: "You are a helpful assistant.",
    tools: [weatherFunction, deleteEntityWithIdFunction]);

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

await app.RunAsync();

[Description("Get the weather for a given location.")]
static string GetWeather(
    [Description("The location to get the weather for.")]
        string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

[Description("Deletes a location by its ID.")]
static string DeleteEntityWithId(
    [Description("The ID of the entity to delete.")]
        string entityId,
    [Description("The type of entity to delete.")]
        DeleteEntity entityType)
{
    switch (entityType)
    {
        case DeleteEntity.Proverb:
            Console.WriteLine($"Deleting proverb with ID: {entityId}");
            return $"Proverb with ID {entityId} has been deleted.";
        case DeleteEntity.Location:
            Console.WriteLine($"Deleting location with ID: {entityId}");
            return $"Location with ID {entityId} has been deleted.";
        default:
            throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
    }
}

enum DeleteEntity
{
    Proverb,
    Location
}