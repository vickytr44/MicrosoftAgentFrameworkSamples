using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AgentAsAGUI;

public class AgentTools()
{
    public static readonly IList<AITool> FunctionTools = [
        AIFunctionFactory.Create(GetWeather),
        AIFunctionFactory.Create(DeleteEntityWithId)
    ];

    [Description("Get the weather for a given location."), DisplayName("Get_Weather")]
    private static WeatherResponse GetWeather(
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

    [Description("Deletes a entity by its ID.")]
    private static string DeleteEntityWithId(
        [Description("The ID of the entity to delete.")]
        string entityId,
        [Description("The type of entity to delete.")]
        DeleteEntity entityType)
    {
        switch (entityType)
        {
            case DeleteEntity.User:
                Console.WriteLine($"Deleting User with ID: {entityId}");
                return $"User with ID {entityId} has been deleted.";
            case DeleteEntity.Location:
                Console.WriteLine($"Deleting Location with ID: {entityId}");
                return $"Location with ID {entityId} has been deleted.";
            default:
                throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
        }
    }

    enum DeleteEntity
    {
        User,
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
}
