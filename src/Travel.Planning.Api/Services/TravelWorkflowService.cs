using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Workflows;
using Workflows.Dto;
using Workflows.Repository;
using Workflows.Services;

namespace Travel.Planning.Api.Services;

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> Execute(WorkflowRequest request);
}

public class TravelWorkflowService(
    ICheckpointRepository repository,
    ITravelPlanService travelPlanService,
    IWorkflowFactory workflowFactory, 
    ILogger<TravelWorkflowService> logger,
    IWorkflowRepository workflowRepository) : ITravelWorkflowService
{
  
    public async Task<WorkflowResponse> Execute(WorkflowRequest request)
    {
        var workflow = await workflowFactory.Create();

        var state = await workflowRepository.LoadAsync(request.Meta.ThreadId);

        await travelPlanService.CreateTravelPlan();

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore2(repository, request.Meta.ThreadId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, logger);

        var serializedRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });

        var response = await travelWorkflow.Execute(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, serializedRequest), request.Meta.ThreadId));

        await workflowRepository.SaveAsync(request.Meta.ThreadId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new WorkflowResponse(response.State, response.Message);
    }
}