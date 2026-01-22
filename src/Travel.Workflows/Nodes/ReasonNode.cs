using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;
using Travel.Workflows.Extensions;
using Travel.Workflows.Observability;
using Travel.Workflows.Services;

namespace Travel.Workflows.Nodes;


public class ReasonNode(AIAgent agent, ITravelService travelService) : ReflectingExecutor<ReasonNode>(WorkflowConstants.ReasonNodeName),
   
    IMessageHandler<ReasoningInputDto, ReasoningOutputDto>
{
    private const string ReasonNodeError = "Reason Node Error";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(), new Extensions.WorkflowExtensions.NullableDateTimeConverter() }
    };

    public async ValueTask<ReasoningOutputDto> HandleAsync(
        ReasoningInputDto reasoningInput, 
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ReasonNodeName}{WorkflowConstants.Observe}");
            
        try
        {
            var threadId = await context.GetThreadId(cancellationToken);

            var travelPlanSummary = await travelService.GetSummary(threadId);

            var template = JsonSerializer.Serialize(new ReasoningState(reasoningInput.Observation, travelPlanSummary));

            var message = new ChatMessage(ChatRole.User, template);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, message.Text);


            var chatOptions = new ChatClientAgentRunOptions()
            {
                ChatOptions = new ChatOptions()
                {
                    AdditionalProperties = new AdditionalPropertiesDictionary()
                    {
                      { "agent_thread_id", threadId }
                    }
                }
            };

            var response = await agent.RunAsync(message, options:chatOptions, cancellationToken: cancellationToken);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, response.Text);

            var reasoningOutput = JsonSerializer.Deserialize<ReasoningOutputDto>(response.Text, SerializerOptions)
                ?? throw new JsonException("Failed to deserialize response to ReasoningOutputDto");

            await context.AddEventAsync(new WorkflowStatusEvent(reasoningOutput.Status, reasoningOutput.Thought, "Travel Planning Agent"), cancellationToken);

            return reasoningOutput;
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(exception.Message, ReasonNodeError, WorkflowConstants.ReasonNodeName, exception), cancellationToken);

            return new ReasoningOutputDto { NextAction = NextAction.Error , Status = exception.Message};
        }
    }
}