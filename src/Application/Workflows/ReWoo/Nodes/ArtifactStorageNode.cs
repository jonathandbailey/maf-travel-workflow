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
    public async ValueTask HandleAsync(ArtifactStorageDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("ArtifactStorageHandleRequest");

        activity?.SetTag("re-woo.node", "artifact_storage_node");

        activity?.SetTag("re-woo.input.message", message.Content);

        var userId = await context.UserId();
        var sessionId = await context.SessionId();

        await artifactRepository.SaveAsync(sessionId, userId, message.Content, message.Key);

        await context.AddEventAsync(new ArtifactStatusEvent($"{message.Key} - Created."), cancellationToken);
    }
}