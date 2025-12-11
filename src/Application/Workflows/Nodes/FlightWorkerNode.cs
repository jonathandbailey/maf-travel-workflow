using Application.Agents;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Application.Workflows.Nodes;

public class FlightWorkerNode(IAgent agent) : 
    ReflectingExecutor<FlightWorkerNode>(WorkflowConstants.FlightWorkerNodeName), 
   
    IMessageHandler<CreatePlanRequestDto>
{
    private const string FlightWorkerNodeError = "Flight Worker Node has failed to execute.";

    public async ValueTask HandleAsync(CreatePlanRequestDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.FlightWorkerNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.FlightWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), cancellationToken: cancellationToken);

            var responseMessage = response.Messages.First();

            WorkflowTelemetryTags.SetInputPreview(activity, responseMessage.Text);

            activity?.SetTag(WorkflowTelemetryTags.ArtifactKey, "flights");

            await context.SendMessageAsync(new ArtifactStorageDto("flights", responseMessage.Text), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(FlightWorkerNodeError, "flights", WorkflowConstants.FlightWorkerNodeName, exception), cancellationToken);
        }
    }
}