using Application.Infrastructure;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace Application.Workflows.ReWoo.Nodes;

public class ArtifactStorageNode(IArtifactRepository artifactRepository) : 
    ReflectingExecutor<ArtifactStorageNode>(WorkflowConstants.ArtifactStorageNodeName), 
    IMessageHandler<ArtifactStorageDto>
{
    private const string ArtifactStorageNodeError = "Artifact Storage Node has failed to execute.";

    public async ValueTask HandleAsync(ArtifactStorageDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ArtifactStorageNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ArtifactStorageNodeName);

        WorkflowTelemetryTags.SetInputPreview(activity, message.Content);

        try
        {
            var userId = await context.UserId();
            var sessionId = await context.SessionId();

            await artifactRepository.SaveAsync(sessionId, userId, message.Content, message.Key);

            await context.AddEventAsync(new ArtifactStatusEvent(message.Key, ArtifactStatus.Created), cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(ArtifactStorageNodeError, message.Key, WorkflowConstants.ArtifactStorageNodeName, exception), cancellationToken);
            await context.AddEventAsync(new ArtifactStatusEvent(message.Key, ArtifactStatus.Error), cancellationToken);
        }
    }
}