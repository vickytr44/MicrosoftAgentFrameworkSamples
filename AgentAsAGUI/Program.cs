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
AIFunction deleteProverbWithIdFunction = AIFunctionFactory.Create(DeleteProverbWithId);
AIFunction deleteLocationWithIdFunction = AIFunctionFactory.Create(DeleteLocationWithId);
//AIFunction approvalRequiredWeatherFunction =
//    new ApprovalRequiredAIFunction(weatherFunction);

var agent = chatClient.AsAIAgent(
    name: "AGUIAssistant",
    instructions: "You are a helpful assistant.",
    tools: [weatherFunction, deleteProverbWithIdFunction, deleteLocationWithIdFunction]);

// Map the AG-UI agent endpoint
app.MapAGUI("/", agent);

await app.RunAsync();

[Description("Get the weather for a given location.")]
static string GetWeather(
    [Description("The location to get the weather for.")]
        string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

[Description("Deletes a proverb by its ID.")]
static string DeleteProverbWithId(
    [Description("The ID of the proverb to delete.")]
        string proverbId)
{
    Console.WriteLine($"Deleting proverb with ID: {proverbId}");
    return $"Proverb with ID {proverbId} has been deleted.";
}

[Description("Deletes a location by its ID.")]
static string DeleteLocationWithId(
    [Description("The ID of the location to delete.")]
        string locationId)
{
    Console.WriteLine($"Deleting location with ID: {locationId}");
    return $"Location with ID {locationId} has been deleted.";
}