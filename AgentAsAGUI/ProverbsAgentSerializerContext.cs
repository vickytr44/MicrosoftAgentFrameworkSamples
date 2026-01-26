using System.Text.Json.Serialization;

public class ProverbsStateSnapshot
{
    [JsonPropertyName("proverbs")]
    public List<string> Proverbs { get; set; } = [];
}

// =================
// Serializer Context
// =================
[JsonSerializable(typeof(ProverbsStateSnapshot))]
internal sealed partial class ProverbsAgentSerializerContext : JsonSerializerContext;