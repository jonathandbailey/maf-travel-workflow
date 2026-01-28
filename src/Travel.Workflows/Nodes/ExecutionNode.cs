using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Observability;
using Travel.Workflows.Services;

namespace Travel.Workflows.Nodes;

public class ExecutionNode(ITravelService travelService) : ReflectingExecutor<ExecutionNode>(WorkflowConstants.ExecutionNode), 
    IMessageHandler<ReasoningOutputDto>,
    IMessageHandler<AgentResponse>
{
    private const string? UnknownNextAction = "Unknown NextAction";

    public async ValueTask HandleAsync(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var threadId = await context.GetThreadId(cancellationToken);

        using var activity = TravelWorkflowTelemetry.InvokeNode(WorkflowConstants.ExecutionNode, threadId);
      
        var serialized = JsonSerializer.Serialize(message);

        activity?.AddNodeInput(serialized);

        if (message.TravelPlanUpdate != null)
        {
            await travelService.UpdateTravelPlan(message.TravelPlanUpdate, threadId);

            await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
        }

        switch (message.NextAction)
        {
            case NextAction.RequestInformation:
                await context.SendMessageAsync(new UserRequest(serialized), cancellationToken: cancellationToken);
                break;
            case NextAction.FlightAgent:
                var plan = await travelService.GetSummary(threadId);
                await context.SendMessageAsync(new CreateFlightOptions(plan, message), cancellationToken: cancellationToken);
                break;
            case NextAction.Error:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message),UnknownNextAction);
        }
    }
    public async ValueTask HandleAsync(AgentResponse agentResponse, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
    }
}

