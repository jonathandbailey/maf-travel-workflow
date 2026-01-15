using Application.Observability;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Workflows.Dto;
using Workflows.Events;
using Workflows.Services;

namespace Workflows.Nodes;

public class ReasonNode(AIAgent agent, ITravelPlanService travelPlanService) : ReflectingExecutor<ReasonNode>(WorkflowConstants.ReasonNodeName),
   
    IMessageHandler<ReasoningInputDto, ReasoningOutputDto>
{
    private const string ReasonNodeError = "Reason Node Error";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async ValueTask<ReasoningOutputDto> HandleAsync(
        ReasoningInputDto reasoningInput, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ReasonNodeName}{WorkflowConstants.Observe}");

            
        try
        {
            var travelPlanSummary = await travelPlanService.GetSummary();

            var template = JsonSerializer.Serialize(new ReasoningState(reasoningInput.Observation, travelPlanSummary));

            var message = new ChatMessage(ChatRole.User, template);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, message.Text);

            var response = await agent.RunAsync(message, cancellationToken: cancellationToken);

            WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, response.Text);

            var reasoningOutput = JsonSerializer.Deserialize<ReasoningOutputDto>(response.Text, SerializerOptions)
                ?? throw new JsonException("Failed to deserialize response to ReasoningOutputDto");

            await context.AddEventAsync(new WorkflowStatusEvent(reasoningOutput.Status, reasoningOutput.Thought, WorkflowConstants.ReasonNodeName), cancellationToken);

            return reasoningOutput;
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(exception.Message, ReasonNodeError, WorkflowConstants.ReasonNodeName, exception), cancellationToken);

            return new ReasoningOutputDto { NextAction = NextAction.Error , Status = exception.Message};
        }
    }
}