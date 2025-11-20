using Application.Observability;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace Application.Workflows.ReWoo.Nodes;

public class FlightWorkerNode() : ReflectingExecutor<FlightWorkerNode>("FlightWorkerNode"), IMessageHandler<OrchestratorWorkerTaskDto>
{
    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("FlightWorkerHandleRequest");

        activity?.SetTag("re-woo.node", "flight_worker_node");

        activity?.SetTag("re-woo.input.message", message);
    }
}