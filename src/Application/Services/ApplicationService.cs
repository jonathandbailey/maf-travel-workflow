using Application.Interfaces;
using Application.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Application.Workflows.Dto;

namespace Application.Services;

public class ApplicationService(
    IWorkflowFactory workflowFactory, 
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

        var response = await travelWorkflow.Execute(
            new TravelWorkflowRequestDto(
                new ChatMessage(ChatRole.User, request.Message),
                request.UserId,
                request.SessionId,
                request.ExchangeId
            ));

        await workflowRepository.SaveAsync(request.UserId, request.SessionId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new ConversationResponse(request.SessionId, request.UserId, response.Message, request.ExchangeId);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}