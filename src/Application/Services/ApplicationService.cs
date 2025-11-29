using Application.Infrastructure;
using Application.Workflows;
using Application.Workflows.ReAct.Dto;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;

namespace Application.Services;

public class ApplicationService(IWorkflowFactory workflowFactory, IWorkflowRepository workflowRepository, ICheckpointRepository repository)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var workflow = await workflowFactory.Create();
        
        var state = await workflowRepository.LoadAsync(request.UserId, request.SessionId);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository, request.UserId, request.SessionId));

        var travelWorkflow = new TravelWorkflow(workflow, checkpointManager, state.CheckpointInfo, state.State);

        var response = await travelWorkflow.Execute(
            new TravelWorkflowRequestDto(
                new ChatMessage(ChatRole.User, request.Message),
                request.UserId,
                request.SessionId
            ));

        await workflowRepository.SaveAsync(request.UserId, request.SessionId, travelWorkflow.State, travelWorkflow.CheckpointInfo);

        return new ConversationResponse(request.SessionId, request.UserId, response.Message);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}