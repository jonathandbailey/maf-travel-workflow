using Application.Observability;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;

namespace Application.Workflows.Nodes;

public class ActNode(ITravelPlanService travelPlanService) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), 
    IMessageHandler<ReasoningOutputDto>,
    IMessageHandler<FlightOptionsCreated, ReasoningInputDto>
{
    public async ValueTask HandleAsync(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ActNodeName}{WorkflowConstants.HandleRequest}");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ActNodeName);
    
        var serialized = JsonSerializer.Serialize(message);

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, serialized);

        if(message.TravelPlanUpdate != null)
        {
            await travelPlanService.UpdateAsync(message.TravelPlanUpdate!);

            await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
        }

        switch (message.NextAction)
        {
            case NextAction.RequestInformation:
                await context.SendMessageAsync(new RequestUserInput(serialized), cancellationToken: cancellationToken);
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
    public async ValueTask<ReasoningInputDto> HandleAsync(FlightOptionsCreated message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);

        var output = $"Flight Options : {message.FlightOptionsStatus}, User Flight Option Status: {message.UserFlightOptionStatus}";

        return new ReasoningInputDto(output, "FlightWorkerInput");
    }
}

