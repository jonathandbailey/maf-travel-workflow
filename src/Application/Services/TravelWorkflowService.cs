using Application.Users;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Text.Json;
using Workflows;
using Workflows.Dto;
using Workflows.Repository;
using Workflows.Services;

namespace Application.Services;

public class TravelWorkflowService(
    IExecutionContextAccessor executionContext, 
    ICheckpointRepository repository,
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

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, logger);

        var serializedRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });

        var response = await travelWorkflow.Execute(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, serializedRequest)));

        await workflowRepository.SaveAsync(executionContext.Context.UserId, executionContext.Context.SessionId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new WorkflowResponse(response.State, response.Message);
    }
}