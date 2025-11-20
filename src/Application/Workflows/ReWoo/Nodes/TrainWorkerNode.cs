using Application.Observability;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace Application.Workflows.ReWoo.Nodes;

public class TrainWorkerNode() : ReflectingExecutor<TrainWorkerNode>("TrainWorkerNode"), IMessageHandler<OrchestratorWorkerTaskDto>
{
    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("TrainWorkerHandleRequest");

        activity?.SetTag("re-woo.node", "train_worker_node");

        activity?.SetTag("re-woo.input.message", message);
    }
}