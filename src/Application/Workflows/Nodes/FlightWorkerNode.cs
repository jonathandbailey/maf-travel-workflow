using Application.Agents;
using Application.Interfaces;
using Application.Models;
using Application.Models.Flights;
using Application.Observability;
using Application.Services;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows.Nodes;

public class FlightWorkerNode(IAgent agent, IArtifactRepository artifactRepository, ITravelPlanService travelPlanService) : 
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
        CancellationToken cancellationToken = default)
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
                var searchArtifact = new ArtifactStorageDto("flights", payload);

                var travelPlan = await travelPlanService.AddFlightSearchOption(new FlightOptionSearch(searchArtifact.Id));

                await context.SendMessageAsync(searchArtifact, cancellationToken: cancellationToken);

                await context.SendMessageAsync(new FlightOptionsCreated(FlightOptionsStatus.Created, UserFlightOptionsStatus.UserChoiceRequired, flightOptions.FlightOptions), cancellationToken: cancellationToken);
            }

            if (flightOptions.Action == "FlightOptionsSelected")
            {
                var flightOption = flightOptions.FlightOptions.Results.First();

                var mapped = MapFlightOption(flightOption);

                await travelPlanService.SelectFlightOption(mapped);
                
                await context.SendMessageAsync(new FlightOptionsCreated(FlightOptionsStatus.Created, UserFlightOptionsStatus.Selected, flightOptions.FlightOptions), cancellationToken: cancellationToken);
            }

        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(FlightWorkerNodeError, "flights", WorkflowConstants.FlightWorkerNodeName, exception), cancellationToken);
        }
    }

    private FlightOption MapFlightOption(FlightOptionDto flightOption)
    {
        return new FlightOption
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = new FlightEndpoint
            {
                Airport = flightOption.Departure.Airport,
                Datetime = flightOption.Departure.Datetime
            },
            Arrival = new FlightEndpoint
            {
                Airport = flightOption.Arrival.Airport,
                Datetime = flightOption.Arrival.Datetime
            },
            Duration = flightOption.Duration,
            Price = new FlightPrice
            {
                Amount = flightOption.Price.Amount,
                Currency = flightOption.Price.Currency
            }
        };
    }
}