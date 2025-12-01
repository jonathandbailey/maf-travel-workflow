using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class FlightWorkerNode(IAgent agent) : 
    ReflectingExecutor<FlightWorkerNode>(WorkflowConstants.FlightWorkerNodeName), 
    IMessageHandler<OrchestratorWorkerTaskDto>
{
    private const string FlightWorkerNodeError = "Flight Worker Node has failed to execute.";

    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.FlightWorkerNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.FlightWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);

            var userId = await context.UserId();
            var sessionId = await context.SessionId();

            var response = await agent.RunAsync( new ChatMessage(ChatRole.User, serialized), sessionId, userId, cancellationToken: cancellationToken);
    
            var responseMessage = response.Messages.First();

            WorkflowTelemetryTags.SetInputPreview(activity, responseMessage.Text);

            activity?.SetTag(WorkflowTelemetryTags.ArtifactKey, message.ArtifactKey);

            await context.SendMessageAsync(new ArtifactStorageDto(message.ArtifactKey, responseMessage.Text), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(FlightWorkerNodeError, message.ArtifactKey, WorkflowConstants.FlightWorkerNodeName,exception), cancellationToken);
        }
    }
}