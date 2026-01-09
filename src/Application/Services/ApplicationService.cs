using Application.Agents;
using Application.Dto;
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
        var parsingAgent = await agentFactory.Create(AgentTypes.Parser);

        var parsingResponse = await parsingAgent.RunAsync(new ChatMessage(ChatRole.User, request.Message), CancellationToken.None);
    
        var response = await travelWorkflowService.PlanVacation(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, parsingResponse.Text)));

        if (response.State == WorkflowState.WaitingForUserInput)
        {
            var userAgent = await agentFactory.Create(AgentTypes.User);

            await foreach (var update in userAgent.RunStreamingAsync(new ChatMessage(ChatRole.Assistant, response.Message), CancellationToken.None))
            {
                await userStreamingService.Stream(update.Text, false);
            }

            await userStreamingService.Stream(string.Empty, true);
        }
 
        return new ConversationResponse(request.SessionId, request.UserId, response.Message, request.ExchangeId);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}