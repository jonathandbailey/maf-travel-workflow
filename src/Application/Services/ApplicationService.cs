using Agents;
using Application.Interfaces;
using Microsoft.Extensions.AI;
using Workflows.Dto;

namespace Application.Services;

public class ApplicationService(
    IAgentFactory agentFactory,
    IUserStreamingService userStreamingService,
    ITravelWorkflowService travelWorkflowService)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var conversationAgent = await agentFactory.CreateConversationAgent(travelWorkflowService.PlanVacation);
    
        await foreach (var update in conversationAgent.RunStreamingAsync(new ChatMessage(ChatRole.Assistant, request.Message)))
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