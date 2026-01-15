using A2A;
using Workflows.Dto;

namespace Travel.Planning.Api.Services;

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
        TaskManager.OnMessageReceived += OnMessageReceived;
    }

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

    private Task<AgentCard> OnAgentCardQuery(string url, CancellationToken cancellationToken)
    {
        return _agentDiscoveryService.GetAgentCard(url);
    }
}

public interface IWorkflowService
{
    ITaskManager TaskManager { get; }
}