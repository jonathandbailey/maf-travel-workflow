using Application.Dto;
using Application.Interfaces;
using Application.Users;
using Application.Workflows;
using Application.Workflows.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Text.Json;

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
    [Description("PlanVacation")]
    public async Task<WorkflowResponse> PlanVacation(WorkflowRequest request)
    {
        var workflow = await workflowFactory.Create();

        var state = await workflowRepository.LoadAsync(executionContext.Context.UserId, executionContext.Context.SessionId);

        await travelPlanService.CreateTravelPlan();

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository, executionContext.Context.UserId, executionContext.Context.SessionId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, userStreamingService, logger);

        var serializedRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });

        var response = await travelWorkflow.Execute(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, serializedRequest)));

        await workflowRepository.SaveAsync(executionContext.Context.UserId, executionContext.Context.SessionId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new WorkflowResponse(response.State, response.Message);
    }
}

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> PlanVacation(WorkflowRequest request);
}