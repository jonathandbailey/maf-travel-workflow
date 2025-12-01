using Application.Agents;
using Application.Observability;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class TrainWorkerNode(IAgent agent) : ReflectingExecutor<TrainWorkerNode>(WorkflowConstants.TrainWorkerNodeName), IMessageHandler<OrchestratorWorkerTaskDto>
{
    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("TrainWorkerHandleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.TrainWorkerNodeName);

        WorkflowTelemetryTags.SetInputPreview(activity, JsonSerializer.Serialize(message));

        var serialized = JsonSerializer.Serialize(message);
    
        activity?.AddEvent(new ActivityEvent("LLMRequestSent"));

        var userId = await context.UserId();
        var sessionId = await context.SessionId();

        var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), sessionId, userId, cancellationToken: cancellationToken);

        activity?.AddEvent(new ActivityEvent("LLMResponseReceived"));

        var responseMessage = response.Messages.First();

        WorkflowTelemetryTags.SetInputPreview(activity, responseMessage.Text);

        activity?.SetTag(WorkflowTelemetryTags.ArtifactKey, message.ArtifactKey);

        await context.SendMessageAsync(new ArtifactStorageDto(message.ArtifactKey, responseMessage.Text), cancellationToken: cancellationToken);
    }
}