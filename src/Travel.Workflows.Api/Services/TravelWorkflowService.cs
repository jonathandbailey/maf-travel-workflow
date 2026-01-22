using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Repository;
using Travel.Workflows.Services;

namespace Travel.Workflows.Api.Services;


public class TravelWorkflowService(
    ICheckpointRepository repository,
    IWorkflowFactory workflowFactory, 
    ILogger<TravelWorkflowService> logger,
    IWorkflowRepository workflowRepository) : ITravelWorkflowService
{

    public async IAsyncEnumerable<WorkflowResponse> Execute(WorkflowRequest request)
    {
        var workflow = await workflowFactory.Create();

        var state = await workflowRepository.LoadAsync(request.Meta.ThreadId);
  
        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository, request.Meta.ThreadId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State, logger);

        await foreach (var response in travelWorkflow.Execute(new TravelWorkflowRequestDto(new ChatMessage(ChatRole.User, request.Meta.RawUserMessage), request.Meta.ThreadId)))
        {
            yield return new WorkflowResponse( response.State, response.Message, response.Action, response.Payload);
        }

        await workflowRepository.SaveAsync(request.Meta.ThreadId, travelWorkflow.State, travelWorkflow.CheckpointInfo);
    }
}

public interface ITravelWorkflowService
{
    IAsyncEnumerable<WorkflowResponse> Execute(WorkflowRequest request);
}
