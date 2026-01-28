using Microsoft.Agents.AI.Workflows;
using Travel.Workflows.Dto;
using Travel.Workflows.Observability;
using Workflows;
using Workflows.Interfaces;

namespace Travel.Workflows.Api.Services;


public class TravelWorkflowService(
    ICheckpointRepository repository,
    IWorkflowFactory workflowFactory, 
    ILogger<TravelWorkflowService> logger,
    IWorkflowRepository workflowRepository) : ITravelWorkflowService
{

    public async IAsyncEnumerable<WorkflowResponse> Execute(WorkflowRequest request)
    {
        using var activity = TravelWorkflowTelemetry.Start($"invoke_workflow TravelPlanning");

        var workflow = await workflowFactory.Create();

        var state = await workflowRepository.LoadAsync(request.ThreadId);
  
        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository, request.ThreadId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, logger);

        await foreach (var response in travelWorkflow.Execute(request))
        {
            yield return response;
        }

        await workflowRepository.SaveAsync(request.ThreadId, travelWorkflow.State, travelWorkflow.CheckpointInfo);
    }
}

public interface ITravelWorkflowService
{
    IAsyncEnumerable<WorkflowResponse> Execute(WorkflowRequest request);
}
