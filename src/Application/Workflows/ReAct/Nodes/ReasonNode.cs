using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace Application.Workflows.ReAct.Nodes;

public class ReasonNode(IAgent agent) : ReflectingExecutor<ReasonNode>(WorkflowConstants.ReasonNodeName) , IMessageHandler<TravelWorkflowRequestDto, ActRequest>,
    IMessageHandler<ActObservation, ActRequest>
{
    private Activity? _activity;
    public async ValueTask<ActRequest> HandleAsync(TravelWorkflowRequestDto requestDto, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Trace(requestDto);

        await context.SessionId(requestDto.SessionId);
        await context.UserId(requestDto.UserId);

        var response = await Process(requestDto.Message, cancellationToken, context);

        TraceEnd();

        return response;
    }

    public async ValueTask<ActRequest> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("ReasonHandleObserve");

        activity?.SetTag("react.node", "reason_node");

        activity?.SetTag("react.observation.response_message", actObservation.Message);

        var message = new ChatMessage(ChatRole.User, actObservation.Message);

        return await Process(message, cancellationToken, context);
    }

    private async Task<ActRequest> Process(ChatMessage message, CancellationToken cancellationToken, IWorkflowContext context)
    {
        _activity?.AddEvent(new ActivityEvent("LLMRequestSent"));

        var userId = await context.ReadStateAsync<Guid>("UserId", scopeName: "Global", cancellationToken);
        var sessionId = await context.ReadStateAsync<Guid>("SessionId", scopeName: "Global", cancellationToken);

        var response = await agent.RunAsync(new List<ChatMessage> { message }, sessionId, userId, cancellationToken);

        _activity?.AddEvent(new ActivityEvent("LLMResponseReceived"));

        _activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        _activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        _activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);

        _activity?.SetTag("react.output.message", response.Messages.First().Text);

        return new ActRequest(response.Messages.First());
    }

    private void Trace(TravelWorkflowRequestDto requestDto)
    {
        _activity = Telemetry.Start("ReasonHandleRequest");

        _activity?.SetTag("react.node", "reason_node");

        _activity?.SetTag("react.input.message", requestDto.Message.Text);
    }

    private void TraceEnd()
    {
        _activity?.Dispose();
    }
}