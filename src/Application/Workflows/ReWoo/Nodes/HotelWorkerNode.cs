using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class HotelWorkerNode(IAgent agent) : 
    ReflectingExecutor<HotelWorkerNode>(WorkflowConstants.HotelWorkerNodeName), 
    IMessageHandler<OrchestratorWorkerTaskDto>
{
    private const string HotelWorkerNodeError = "Hotel Worker Node has failed to execute.";

    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.HotelWorkerNodeName}.handleRequest");
        
        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.HotelWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);
  
            var userId = await context.UserId();
            var sessionId = await context.SessionId();

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), sessionId, userId, cancellationToken: cancellationToken);
   
            activity?.SetTag("re-woo.output.message", response.Text);

            WorkflowTelemetryTags.SetOutputPreview(activity, response.Text);

            await context.SendMessageAsync(new ArtifactStorageDto(message.ArtifactKey, response.Text), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(HotelWorkerNodeError, message.ArtifactKey, WorkflowConstants.HotelWorkerNodeName, exception), cancellationToken);
        }
    }
}