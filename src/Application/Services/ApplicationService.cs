using Application.Agents;
using Application.Infrastructure;
using Application.Observability;
using Application.Workflows;
using Application.Workflows.ReAct;
using Application.Workflows.ReAct.Dto;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI.Workflows;

namespace Application.Services;

public class ApplicationService(IAgentFactory agentFactory, IWorkflowRepository workflowRepository, ICheckpointRepository repository)
    : IApplicationService
{
    public async Task<ConversationResponse> Execute(ConversationRequest request)
    {
        var initializeActivity = Telemetry.Start("Initialize");

        var reasonAgent = await agentFactory.CreateReasonAgent();

        var actAgent = await agentFactory.CreateActAgent();
      
        initializeActivity?.Dispose();

        var workflowActivity = Telemetry.Start("Workflow");

        workflowActivity?.SetTag("User Input", request.Message);
   
        var state = await workflowRepository.LoadAsync(request.SessionId);

        var checkpointManager = CheckpointManager.CreateJson(new CheckpointStore(repository));

        var workflow = new ReActWorkflow(reasonAgent, actAgent, checkpointManager,state.CheckpointInfo, state.State);

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