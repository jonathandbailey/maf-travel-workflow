using A2A;
using Azure;
using System.Text;
using System.Threading.Tasks;
using Travel.Planning.Api.Services;
using Travel.Workflows.Dto;

namespace Travel.Workflows.Api.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IAgentDiscoveryService _agentDiscoveryService;
    private readonly ITravelWorkflowService _travelWorkflowService;

    public ITaskManager TaskManager { get; } = new TaskManager();

    public WorkflowService(IAgentDiscoveryService agentDiscoveryService, ITravelWorkflowService travelWorkflowService)
    {
        _agentDiscoveryService = agentDiscoveryService;
        _travelWorkflowService = travelWorkflowService;
        
        TaskManager.OnAgentCardQuery+= OnAgentCardQuery;
        //TaskManager.OnMessageReceived += OnMessageReceived;
        TaskManager.OnTaskCreated += OnTaskCreated;
    }

    private async Task OnTaskCreated(AgentTask agentTask, CancellationToken cancellationToken)
    {
        var messageText = agentTask.History.OfType<AgentMessage>().First().Parts.OfType<TextPart>().First().Text;

        var workflowRequest = new WorkflowRequest
        {
            Meta =
            {
                ThreadId = agentTask.ContextId,
                RawUserMessage = messageText
            }
        };

        await foreach (var response in _travelWorkflowService.Execute(workflowRequest))
        {
            if (response.State == WorkflowState.WaitingForUserInput)
            {
                var message = new AgentMessage
                {
                    Role = MessageRole.Agent,
                    ContextId = agentTask.ContextId,
                    Parts = new List<Part> { new TextPart() {Text = response.Message}}
                };

                await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.InputRequired, message, final: false, cancellationToken);
            }

            if (response.State == WorkflowState.Executing && response.Action == WorkflowAction.StatusUpdate)
            {
                var message = new AgentMessage
                {
                    Role = MessageRole.Agent,
                    ContextId = agentTask.ContextId,
                    Parts = new List<Part> { new TextPart() { Text = response.Message } }
                };

                await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Working, message, final: false, cancellationToken);
            }
        }

        /* 
        var artifact = new Artifact
        {
            Parts =
            [
                new TextPart()
                {
                    Text = workResponse.Message
                }
            ]
        };

        await TaskManager.ReturnArtifactAsync(agentTask.Id, artifact, cancellationToken);
        */
        var finalMessage = new AgentMessage
        {
            Role = MessageRole.Agent,
            ContextId = agentTask.ContextId,
            Parts = new List<Part> { new TextPart() { Text = "Travel Workflow Completed." } }
        };

        // Send final completion status
        await TaskManager.UpdateStatusAsync(agentTask.Id, TaskState.Completed, finalMessage, final: true, cancellationToken);
    }
    /*
    private async Task<A2AResponse> OnMessageReceived(MessageSendParams messageSendParams, CancellationToken cancellationToken)
    {
        var messageText = messageSendParams.Message.Parts.OfType<TextPart>().First().Text;

        var workflowRequest = new WorkflowRequest
        {
            Meta =
            {
                ThreadId = messageSendParams.Message.ContextId!,
                RawUserMessage = messageText
            }
        };

        var workResponse = await _travelWorkflowService.Execute(workflowRequest);

        var message = new AgentMessage()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = [new TextPart() {
                Text = workResponse.Message
            }]
        };

        return message;
    }
    */
    private Task<AgentCard> OnAgentCardQuery(string url, CancellationToken cancellationToken)
    {
        return _agentDiscoveryService.GetAgentCard(url);
    }
}

public interface IWorkflowService
{
    ITaskManager TaskManager { get; }
}