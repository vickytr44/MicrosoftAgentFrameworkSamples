using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace Agent.Shared;

public static class Client
{
    public static IChatClient AzureChatClient(string appName)
    {
        var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new ArgumentNullException("Please set the AZURE_OPENAI_API_KEY environment variable.");
        //var key = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY_GPT5") ?? throw new ArgumentNullException("Please set the AZURE_OPENAI_API_KEY environment variable.");

        AzureKeyCredential credential = new(key);

        var chatClient = new AzureOpenAIClient(
            new Uri("https://devonai.openai.azure.com/"), credential)
            //new Uri("https://n8n-models98136.cognitiveservices.azure.com/"), credential)
               .GetChatClient("gpt-4")
               //.GetChatClient("gpt-5-mini-2")
               .AsIChatClient() // Converts a native OpenAI SDK ChatClient into a Microsoft.Extensions.AI.IChatClient
               .AsBuilder()
               .UseOpenTelemetry(sourceName: appName, configure: (cfg) => cfg.EnableSensitiveData = true)    // Enable OpenTelemetry instrumentation with sensitive data
               .Build();

        return chatClient;
    }
}
