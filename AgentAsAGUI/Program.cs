using Agent.Shared;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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

[Description("Get the weather for a given location."), DisplayName("Get_Weather")]
static WeatherResponse GetWeather(
    [Description("The location to get the weather for."), Required]
        string location)
    => new WeatherResponse
    {
        Location = location,
        Temperature = "15°C",
        Condition = WeatherCondition.Cloudy,
        WindSpeed = "10 km/h",
        Humidity = "80%"
    };

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

public enum WeatherCondition
{
    Sunny,
    Rainy,
    Cloudy,
    Snowy
}

public record WeatherResponse
{
    public string Location { get; init; }
    public string Temperature { get; init; }

    public WeatherCondition Condition { get; init; }

    public string WindSpeed { get; init; }

    public string Humidity { get; init; }
}