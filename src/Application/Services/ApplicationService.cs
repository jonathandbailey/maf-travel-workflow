using Application.Agents;
using Application.Dto;
using Application.Interfaces;
using Application.Workflows;
using Application.Workflows.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ApplicationService(
    IWorkflowFactory workflowFactory, 
    IAgentFactory agentFactory,
    IWorkflowRepository workflowRepository, 
    ICheckpointRepository repository, 
    ITravelPlanService travelPlanService,
    IUserStreamingService userStreamingService,
    ILogger<ApplicationService> logger)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var workflow = await workflowFactory.Create();
        
        var state = await workflowRepository.LoadAsync(request.UserId, request.SessionId);

        await travelPlanService.CreateTravelPlan();

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository, request.UserId, request.SessionId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, userStreamingService, logger);

        var parsingAgent = await agentFactory.Create(AgentTypes.Parser);

        var parsingResponse = await parsingAgent.RunAsync(new ChatMessage(ChatRole.User, request.Message), CancellationToken.None);

        var response = await travelWorkflow.Execute(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, parsingResponse.Text)));

        if (response.State == WorkflowState.WaitingForUserInput)
        {
            var userAgent = await agentFactory.Create(AgentTypes.User);

            await foreach (var update in userAgent.RunStreamingAsync(new ChatMessage(ChatRole.Assistant, response.Message), CancellationToken.None))
            {
                await userStreamingService.Stream(update.Text, false);
            }

            await userStreamingService.Stream(string.Empty, true);
        }
        
        await workflowRepository.SaveAsync(request.UserId, request.SessionId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new ConversationResponse(request.SessionId, request.UserId, response.Message, request.ExchangeId);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}