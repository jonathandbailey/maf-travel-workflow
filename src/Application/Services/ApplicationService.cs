using Application.Agents;
using Application.Infrastructure;
using Application.Observability;
using Application.Workflows.Conversations;
using Microsoft.Extensions.AI;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;

namespace Application.Services;

public class ApplicationService(IAgentFactory agentFactory, IWorkflowRepository workflowRepository, ICheckpointRepository repository)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var initializeActivity = Telemetry.StarActivity("Initialize");

        var reasonAgent = await agentFactory.CreateReasonAgent();

        var actAgent = await agentFactory.CreateActAgent();
      
        initializeActivity?.Dispose();

        var workflowActivity = Telemetry.StarActivity("Workflow");

        workflowActivity?.SetTag("User Input", request.Message);
   
        var state = await workflowRepository.LoadAsync(request.SessionId);

        var checkpointManager = CheckpointManager.CreateJson(new ConversationCheckpointStore(repository));

        var workflow = new ConversationWorkflow(reasonAgent, actAgent, checkpointManager,state.CheckpointInfo, state.State);

        var response = await workflow.Execute(new ChatMessage(ChatRole.User, request.Message));

        workflowActivity?.Dispose();

        await workflowRepository.SaveAsync(request.SessionId, workflow.State, workflow.CheckpointInfo);

        return new ConversationResponse(request.SessionId, response.Message);
    }
}

public interface IApplicationService
{
    Task<ConversationResponse> Execute(ConversationRequest request);
}