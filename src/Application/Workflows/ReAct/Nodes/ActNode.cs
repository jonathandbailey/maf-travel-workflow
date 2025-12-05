using Application.Agents;
using Application.Observability;
using Application.Services;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;

namespace Application.Workflows.ReAct.Nodes;

public class ActNode(IAgent agent, ITravelPlanService travelPlanService) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), IMessageHandler<ActRequest>
{
    private const string NoJsonReturnedByAgent = "Agent/LLM did not return formnatted JSON for routing/actions";
    private const string AgentJsonParseFailed = "Agent JSON parse failed";

    private const string StatusExecuting = "Executing...";

    public async ValueTask HandleAsync(ActRequest message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.AddEventAsync(new WorkflowStatusEvent(StatusExecuting), cancellationToken);

        using var activity = Telemetry.Start($"{WorkflowConstants.ActNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ActNodeName);

        var serialized = JsonSerializer.Serialize(message);

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, serialized);

        switch (message.NextAction)
        {
            case "AskUser":
                await context.SendMessageAsync(new ActUserRequest(serialized), cancellationToken: cancellationToken);
                break;
            case "UpdateTravelPlan":
                await UpdateTravelPlan(message, context, cancellationToken);
                break;
        }
    }

    private async Task UpdateTravelPlan(ActRequest message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await travelPlanService.UpdateAsync(message.TravelPlanUpdate!);

        await context.SendMessageAsync(new ActObservation("Travel Plan Updated", "TravelPlanUpdate"), cancellationToken: cancellationToken);
    }
}

