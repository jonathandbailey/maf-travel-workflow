using Application.Agents;
using Application.Interfaces;
using Application.Workflows.Dto;
using Microsoft.Extensions.AI;

namespace Application.Services;

public class ApplicationService(
    IAgentFactory agentFactory,
    IUserStreamingService userStreamingService,
    ITravelWorkflowService travelWorkflowService)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var conversationAgent = await agentFactory.CreateConversationAgent(travelWorkflowService);
    
        await foreach (var update in conversationAgent.RunStreamingAsync(new ChatMessage(ChatRole.Assistant, request.Message), CancellationToken.None))
        {
            await userStreamingService.Stream(update.Text, false);
        }

        await userStreamingService.Stream(string.Empty, true);
       
 
        return new ConversationResponse(request.SessionId, request.UserId, string.Empty, request.ExchangeId);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}