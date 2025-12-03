using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace Application.Workflows.ReAct.Nodes;

public class ReasonNode(IAgent agent) : ReflectingExecutor<ReasonNode>(WorkflowConstants.ReasonNodeName),
    IMessageHandler<TravelWorkflowRequestDto, ActRequest>,
    IMessageHandler<ActObservation, ActRequest>
{
    private const string StatusThinking = "Evaluating Travel Requirements...";

    public async ValueTask<ActRequest> HandleAsync(
        TravelWorkflowRequestDto requestDto, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ReasonNodeName}.handleRequest");

        await context.AddEventAsync(new WorkflowStatusEvent(StatusThinking), cancellationToken);

        Annotate(activity, requestDto.Message.Text);

        await context.SessionState(new SessionState(requestDto.SessionId, requestDto.UserId, requestDto.RequestId));
   
        return await RunReasoningAsync(requestDto.Message, context, activity, cancellationToken);
    }

    public async ValueTask<ActRequest> HandleAsync(
        ActObservation actObservation, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ReasonNodeName}.observe");

        await context.AddEventAsync(new WorkflowStatusEvent(StatusThinking), cancellationToken);

        Annotate(activity, actObservation.Message);
  
        var message = new ChatMessage(ChatRole.User, actObservation.Message);

        return await RunReasoningAsync(message, context, activity, cancellationToken);
    }

    private async Task<ActRequest> RunReasoningAsync(ChatMessage message, IWorkflowContext context, Activity? activity, CancellationToken cancellationToken)
    {
        var sessionState = await context.SessionState();

        var response = await agent.RunAsync(message, sessionState.SessionId, sessionState.UserId, cancellationToken);

        Annotate(activity, response.Text);

        return new ActRequest(response.Messages.First());
    }

    private static void Annotate(Activity? activity, string preview)
    {
        if (activity == null) return;

        activity.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ReasonNodeName);
        WorkflowTelemetryTags.SetInputPreview(activity, preview);
    }
}