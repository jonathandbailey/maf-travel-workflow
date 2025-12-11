using Application.Observability;
using Application.Services;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;
using Application.Workflows.Dto;

namespace Application.Workflows.Nodes;

public class ActNode(ITravelPlanService travelPlanService) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), IMessageHandler<ReasoningOutputDto>
{
    private const string StatusExecuting = "Executing...";

    public async ValueTask HandleAsync(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.AddEventAsync(new WorkflowStatusEvent(StatusExecuting), cancellationToken);

        using var activity = Telemetry.Start($"{WorkflowConstants.ActNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ActNodeName);

        await context.AddEventAsync(new WorkflowStatusEvent(message.Status), cancellationToken);

        var serialized = JsonSerializer.Serialize(message);

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, serialized);

        if(message.TravelPlanUpdate != null)
        {
            await UpdateTravelPlan(message, context, cancellationToken);
        }

        switch (message.NextAction)
        {
            case "AskUser":
                await context.SendMessageAsync(new RequestUserInput(serialized), cancellationToken: cancellationToken);
                break;
            case "GenerateTravelPlanArtifacts":
                var plan = await travelPlanService.LoadAsync();
                await context.SendMessageAsync(new CreatePlanRequestDto(plan), cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task UpdateTravelPlan(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await travelPlanService.UpdateAsync(message.TravelPlanUpdate!);

        await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
    }
}

