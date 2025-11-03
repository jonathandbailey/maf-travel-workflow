using System.ClientModel;
using Azure.AI.OpenAI;
using ConsoleApp.Settings;
using ConsoleApp.Workflows.Conversations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;

namespace ConsoleApp.Services;

public class Application(IOptions<LanguageModelSettings> settings, IPromptService promptService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var chatClient = new AzureOpenAIClient(new Uri(settings.Value.EndPoint),
                new ApiKeyCredential(
                    settings.Value.ApiKey))
            .GetChatClient(settings.Value.DeploymentName);

        var reasonAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Instructions = promptService.GetPrompt("Reason-Agent")
        });

        var actAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions
        {
            Instructions = promptService.GetPrompt("Act-Agent")
        });

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write(">");
            
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput)) continue;   
      
            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
       
            var conversationWorkflow = new ConversationWorkflow(reasonAgent, actAgent);

            await conversationWorkflow.Execute(userInput, cancellationToken);

            Console.WriteLine();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}