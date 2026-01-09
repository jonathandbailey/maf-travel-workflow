using Application.Dto;
using Application.Interfaces;
using Application.Users;
using Application.Workflows;
using Application.Workflows.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace Application.Services;



public class TravelWorkflowService(
    IExecutionContextAccessor executionContext, 
    ICheckpointRepository repository,
    IUserStreamingService userStreamingService,
    ITravelPlanService travelPlanService,
    IWorkflowFactory workflowFactory, 
    ILogger<TravelWorkflowService> logger,
    IWorkflowRepository workflowRepository) : ITravelWorkflowService
{
    public async Task<WorkflowResponse> PlanVacation(TravelWorkflowRequestDto request)
    {
        var workflow = await workflowFactory.Create();

        var state = await workflowRepository.LoadAsync(executionContext.Context.UserId, executionContext.Context.SessionId);

        await travelPlanService.CreateTravelPlan();

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository, executionContext.Context.UserId, executionContext.Context.SessionId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, userStreamingService, logger);

        var response = await travelWorkflow.Execute(request);

        await workflowRepository.SaveAsync(executionContext.Context.UserId, executionContext.Context.SessionId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new WorkflowResponse(response.State, response.Message);
    }
}

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> PlanVacation(TravelWorkflowRequestDto request);
}