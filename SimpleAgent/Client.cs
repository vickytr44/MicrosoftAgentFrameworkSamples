using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace SimpleAgent;

public static class Client
{
    public static IChatClient AzureChatClient(string appName)
    {
        var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new ArgumentNullException("Please set the AZURE_OPENAI_API_KEY environment variable.");

        AzureKeyCredential credential = new(key);

        var chatClient = new AzureOpenAIClient(
            new Uri("https://devonai.openai.azure.com/"), credential)
                .GetChatClient("gpt-4")
                .AsIChatClient() // Converts a native OpenAI SDK ChatClient into a Microsoft.Extensions.AI.IChatClient
                .AsBuilder()
                .UseOpenTelemetry(sourceName: appName, configure: (cfg) => cfg.EnableSensitiveData = true)    // Enable OpenTelemetry instrumentation with sensitive data
                .Build();

        return chatClient;
    }
}
