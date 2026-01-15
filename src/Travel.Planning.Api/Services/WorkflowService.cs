using A2A;

namespace Travel.Planning.Api.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IAgentDiscoveryService _agentDiscoveryService;

    public ITaskManager TaskManager { get; } = new TaskManager();

    public WorkflowService(IAgentDiscoveryService agentDiscoveryService)
    {
        _agentDiscoveryService = agentDiscoveryService;
        TaskManager.OnAgentCardQuery+= OnAgentCardQuery;
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