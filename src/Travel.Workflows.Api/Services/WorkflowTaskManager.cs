using System.Text.Json;
using A2A;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Workflows;

namespace Travel.Workflows.Api.Services;

public class WorkflowTaskManager : IWorkflowTaskManager
{
    private readonly IAgentDiscoveryService _agentDiscoveryService;
    private readonly ITravelWorkflowService _travelWorkflowService;

    public ITaskManager TaskManager { get; } = new TaskManager();

    public WorkflowTaskManager(IAgentDiscoveryService agentDiscoveryService, ITravelWorkflowService travelWorkflowService)
    {
        _agentDiscoveryService = agentDiscoveryService;
        _travelWorkflowService = travelWorkflowService;
        
        TaskManager.OnAgentCardQuery+= OnAgentCardQuery;
        TaskManager.OnTaskCreated += OnTaskCreated;
    }

    private async Task OnTaskCreated(AgentTask agentTask, CancellationToken cancellationToken)
    {
        var messageText = agentTask.History.OfType<AgentMessage>().First().Parts.OfType<TextPart>().First().Text;

        var request = new WorkflowRequest(new ChatMessage(ChatRole.User, messageText), Guid.Parse(agentTask.ContextId));

        await foreach (var response in _travelWorkflowService.Execute(request))
        {
            if (response.State == WorkflowState.WaitingForUserInput)
            {
                var message = new AgentMessage
                {
                    Role = MessageRole.Agent,
                    ContextId = agentTask.ContextId,
                    Parts = new List<Part> { new TextPart() {Text = response.Message}}
                };

                await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.InputRequired, message, final: true, cancellationToken);
            }

            if (response is { State: WorkflowState.Executing, Action: WorkflowAction.StatusUpdate })
            {
                var message = new AgentMessage
                {
                    Role = MessageRole.Agent,
                    ContextId = agentTask.ContextId,
                    Parts = [new DataPart {  Data = new Dictionary<string, JsonElement>
                    {
                        ["status"] = response.Payload!.Value,
                    }}]
                };

                await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Working, message, final: false, cancellationToken);
            }
            if (response.State == WorkflowState.Completed)
            {
                var message = new AgentMessage
                {
                    Role = MessageRole.Agent,
                    ContextId = agentTask.ContextId,
                    Parts = new List<Part> { new TextPart() { Text = "Workflow Completed." } }
                };

                await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, message, final: true, cancellationToken);
            }

            if (response is { State: WorkflowState.Executing, Action: WorkflowAction.ArtifactCreated })
            {
                var artifact = new Artifact
                {
                    Parts =
                    [new DataPart {  Data = new Dictionary<string, JsonElement>
                    {
                        ["artifact"] = response.Payload!.Value,
                    }}]
                };

                await TaskManager.ReturnArtifactAsync(agentTask.Id, artifact, cancellationToken);
            }
        }
    }
   
    private Task<AgentCard> OnAgentCardQuery(string url, CancellationToken cancellationToken)
    {
        return _agentDiscoveryService.GetAgentCard(url);
    }
}

public interface IWorkflowTaskManager
{
    ITaskManager TaskManager { get; }
}