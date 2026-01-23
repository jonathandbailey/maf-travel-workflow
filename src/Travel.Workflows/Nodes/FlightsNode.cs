using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Observability;
using Travel.Workflows.Services;


namespace Travel.Workflows.Nodes;

public class FlightsNode(AIAgent agent, IFlightService flightService) : 
    ReflectingExecutor<FlightsNode>(WorkflowConstants.FlightNodeName), 
   
    IMessageHandler<CreateFlightOptions, AgentResponse>
{
    private const string FlightWorkerNodeError = "Flight Worker Node has failed to execute.";
    private const string FlightAgent = "Flight Agent";
    private const string FlightsOptionsCreated = "Flights Options Created";
    private const string FlightOptionSelected = "Flight Option Selected";
    private const string FailedToDeserializeFlightOptionsInFlightWorkerNode = "Failed to deserialize flight options in Flight Worker Node";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask<AgentResponse> HandleAsync(CreateFlightOptions message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.FlightNodeName}{WorkflowConstants.HandleRequest}");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.FlightNodeName);

        try
        {
            var serialized = JsonSerializer.Serialize(message);

            WorkflowTelemetryTags.SetInputPreview(activity, serialized);

            var threadId = await context.GetThreadId(cancellationToken);

            var chatOptions = threadId.ToChatClientAgentRunOptions();

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, serialized), options: chatOptions, cancellationToken: cancellationToken);
   
            WorkflowTelemetryTags.SetOutputPreview(activity, response.Text);
       
            var flightSearchResults = JsonSerializer.Deserialize<FlightActionResultDto>(response.Text, SerializerOptions);

            if (flightSearchResults == null)
                throw new JsonException(FailedToDeserializeFlightOptionsInFlightWorkerNode);
      
            switch (flightSearchResults.Action)
            {
                case FlightAction.FlightOptionsCreated:
                {
                    var id =  await flightService.SaveFlightSearch(flightSearchResults.Results, threadId);

                    await context.AddEventAsync(new ArtifactStatusEvent(id, flightSearchResults.Results.ArtifactKey, ArtifactStatus.Created), cancellationToken);

                    return new AgentResponse(FlightAgent, FlightsOptionsCreated, AgentResponseStatus.Success);
                }
                
                case FlightAction.FlightOptionsSelected:
                {
                    await flightService.SaveFlightOption(flightSearchResults.Results, threadId);

                    return new AgentResponse(FlightAgent, FlightOptionSelected, AgentResponseStatus.Success);
                }
                
                default:
                    throw new ArgumentException("Action value is not valid.", nameof(flightSearchResults));
            }
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(exception.Message, FlightWorkerNodeError, WorkflowConstants.FlightNodeName, exception), cancellationToken);

            return new AgentResponse(FlightAgent, exception.Message, AgentResponseStatus.Error);
        }
    }
}