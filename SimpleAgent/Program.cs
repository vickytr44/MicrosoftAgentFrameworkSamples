using SimpleAgent;
using SimpleAgent.Agents;

var appName = "DemoApp";

//Otel.Configure(appName, out TracerProvider? tracerProvider, out MeterProvider? meterProvider, out ILoggerFactory loggerFactory);

var chatClient = Client.AzureChatClient(appName);

//await TestAgent.Run(chatClient);

await HumanInTheLoop.Run(chatClient);

//await AgentWithStructuredOutput.Run(chatClient);

//await AgentAsFunction.Run(chatClient);

//await SequentialWorkflow.Run();

//await ConcurrentWorkflow.Run(chatClient);