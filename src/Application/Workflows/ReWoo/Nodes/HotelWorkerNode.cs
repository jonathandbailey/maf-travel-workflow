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
    IMessageHandler<CreatePlanRequestDto>
{
    private const string HotelWorkerNodeError = "Hotel Worker Node has failed to execute.";
    private const string StatusFindingHotels = "Finding Hotels...";
    private const string StatusHotelsFound = "Hotels Found...";

    public async ValueTask HandleAsync(CreatePlanRequestDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.HotelWorkerNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.HotelWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);

            await context.AddEventAsync(new WorkflowStatusEvent(StatusFindingHotels), cancellationToken);

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), cancellationToken: cancellationToken);

            WorkflowTelemetryTags.SetOutputPreview(activity, response.Text);

            await context.AddEventAsync(new WorkflowStatusEvent(StatusHotelsFound), cancellationToken);

            await context.SendMessageAsync(new ArtifactStorageDto("hotels", response.Text), cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(HotelWorkerNodeError, "hotels", WorkflowConstants.HotelWorkerNodeName, exception), cancellationToken);
        }
    }
}