using Azure.AI.OpenAI;
using ConsoleApp.Settings;
using ConsoleApp.Workflows;
using ConsoleApp.Workflows.Conversations;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;

namespace ConsoleApp.Services;

public class Application(IOptions<LanguageModelSettings> settings, IPromptService promptService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var sessionId = Guid.NewGuid();

        var checkpointManager = CheckpointManager.CreateInMemory();

        var checkPointStore = new CheckpointStore();
        
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

        var workflowState = ConversationWorkflowState.Started;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write(">");
            
            var userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput)) continue;   
      
            if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
       
            var conversationWorkflow = new ConversationWorkflow(reasonAgent, actAgent, checkPointStore, checkpointManager);

            var workflowRequest = new ConversationWorkFlowRequest()
                { Message = userInput, State = workflowState, SessionId = sessionId};
            
            var response = await conversationWorkflow.Execute(workflowRequest, cancellationToken);

            if (response.State == ConversationWorkflowState.AssistantRequest)
            {
                workflowState = ConversationWorkflowState.UserResponse;
            }

            Console.WriteLine();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}