using Application.Observability;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text.Json;
using Application.Models;

namespace Application.Workflows.Nodes;

public class ActNode(ITravelPlanService travelPlanService) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), 
    IMessageHandler<ReasoningOutputDto>,
    IMessageHandler<FlightOptionsCreated>
{
    public async ValueTask HandleAsync(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ActNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ActNodeName);
    
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
            case "HandleFlightOptions":
                var plan = await travelPlanService.LoadAsync();
                await context.SendMessageAsync(new CreateFlightOptions(plan, message), cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task UpdateTravelPlan(ReasoningOutputDto message, IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        await travelPlanService.UpdateAsync(message.TravelPlanUpdate!);

        await context.AddEventAsync(new TravelPlanUpdatedEvent(), cancellationToken);
    }

    public async ValueTask HandleAsync(FlightOptionsCreated message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var plan = await travelPlanService.LoadAsync();

        plan.FlightOptionsStatus = message.FlightOptionsStatus;
        plan.UserFlightOptionStatus = message.UserFlightOptionStatus;

        await travelPlanService.SaveAsync(plan);

        var output = $"Flight Options : {message.FlightOptionsStatus}, User Flight Option Status: {message.UserFlightOptionStatus}";

        await context.SendMessageAsync(new ReasoningInputDto(output, "FlightWorkerInput"), cancellationToken: cancellationToken);
    }
}

