using System.Text.Json;
using Application.Observability;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Observability;
using Travel.Workflows.Services;
using WorkflowTelemetryTags = Travel.Workflows.Observability.WorkflowTelemetryTags;

namespace Travel.Workflows.Nodes;

public class ActNode(ITravelPlanService travelPlanService) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), 
    IMessageHandler<ReasoningOutputDto>,
    IMessageHandler<AgentResponse>
{
    public async ValueTask HandleAsync(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ActNodeName}{WorkflowConstants.HandleRequest}");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ActNodeName);
    
        var serialized = JsonSerializer.Serialize(message);

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, serialized);

        var threadId = await context.ReadStateAsync<string>("agent_thread_id", scopeName: "workflow", cancellationToken);


        if (message.TravelPlanUpdate != null)
        {
            await travelPlanService.UpdateTravelPlanFromEndpoint(message.TravelPlanUpdate!, Guid.Parse(threadId!));

            await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
        }

        switch (message.NextAction)
        {
            case NextAction.RequestInformation:
                await context.SendMessageAsync(new UserRequest(serialized), cancellationToken: cancellationToken);
                break;
            case NextAction.FlightAgent:
                var plan = await travelPlanService.LoadAsync();
                await context.SendMessageAsync(new CreateFlightOptions(plan, message), cancellationToken: cancellationToken);
                break;
            case NextAction.Error:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message),"Unknown NextAction");
        }
    }
    public async ValueTask HandleAsync(AgentResponse agentResponse, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
    }
}

