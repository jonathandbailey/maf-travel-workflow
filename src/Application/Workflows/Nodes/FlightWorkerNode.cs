using Application.Agents;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces;
using Application.Models;

namespace Application.Workflows.Nodes;

public class FlightWorkerNode(IAgent agent, IArtifactRepository artifactRepository) : 
    ReflectingExecutor<FlightWorkerNode>(WorkflowConstants.FlightWorkerNodeName), 
   
    IMessageHandler<CreateFlightOptions>
{
    private const string FlightWorkerNodeError = "Flight Worker Node has failed to execute.";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async ValueTask HandleAsync(CreateFlightOptions message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.FlightWorkerNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.FlightWorkerNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);

            if (await artifactRepository.FlightsExistsAsync())
            {
                var flights = await artifactRepository.GetFlightPlanAsync();
            }

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), cancellationToken: cancellationToken);

            var responseMessage = response.Messages.First();

            WorkflowTelemetryTags.SetInputPreview(activity, responseMessage.Text);

            activity?.SetTag(WorkflowTelemetryTags.ArtifactKey, "flights");

            var flightOptions = response.Deserialize<FlightActionResultDto>(JsonSerializerOptions.Web);

            if (flightOptions == null)
                throw new JsonException("Failed to deserialize flight options in Flight Worker Node");

            var payload = JsonSerializer.Serialize(flightOptions.FlightOptions, SerializerOptions);

            if (flightOptions.Action == "FlightOptionsCreated")
            {
                await context.SendMessageAsync(new ArtifactStorageDto("flights", payload), cancellationToken: cancellationToken);

                await context.SendMessageAsync(new FlightOptionsCreated(FlightOptionsStatus.Created, UserFlightOptionsStatus.UserChoiceRequired), cancellationToken: cancellationToken);
            }

            if (flightOptions.Action == "FlightOptionsSelected")
            {
                await context.SendMessageAsync(new FlightOptionsCreated(FlightOptionsStatus.Created, UserFlightOptionsStatus.Selected), cancellationToken: cancellationToken);
            }

        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(FlightWorkerNodeError, "flights", WorkflowConstants.FlightWorkerNodeName, exception), cancellationToken);
        }
    }
}