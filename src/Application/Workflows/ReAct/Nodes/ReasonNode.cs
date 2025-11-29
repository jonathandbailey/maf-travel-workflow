using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace Application.Workflows.ReAct.Nodes;

public class ReasonNode(IAgent agent) : ReflectingExecutor<ReasonNode>("ReasonNode") , IMessageHandler<TravelWorkflowRequestDto, ActRequest>,
    IMessageHandler<ActObservation, ActRequest>
{
    
    public async ValueTask<ActRequest> HandleAsync(TravelWorkflowRequestDto requestDto, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start("ReasonHandleRequest");

        activity?.SetTag("react.node", "reason_node");

        activity?.SetTag("react.input.message", requestDto.Message.Text);

        await context.QueueStateUpdateAsync("SessionId", requestDto.SessionId, scopeName:"Global", cancellationToken);
        await context.QueueStateUpdateAsync("UserId", requestDto.UserId, scopeName:"Global", cancellationToken);

        return await Process(requestDto.Message, activity, cancellationToken, context);
    }

    public async ValueTask<ActRequest> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("ReasonHandleObserve");

        activity?.SetTag("react.node", "reason_node");

        activity?.SetTag("react.observation.response_message", actObservation.Message);

        var message = new ChatMessage(ChatRole.User, actObservation.Message);

        return await Process(message, activity, cancellationToken, context);
    }

    private async Task<ActRequest> Process(ChatMessage message, Activity? activity, CancellationToken cancellationToken, IWorkflowContext context)
    {
        activity?.AddEvent(new ActivityEvent("LLMRequestSent"));

        var userId = await context.ReadStateAsync<Guid>("UserId", scopeName: "Global", cancellationToken);
        var sessionId = await context.ReadStateAsync<Guid>("SessionId", scopeName: "Global", cancellationToken);

        var response = await agent.RunAsync(new List<ChatMessage> { message }, sessionId, userId, cancellationToken);

        activity?.AddEvent(new ActivityEvent("LLMResponseReceived"));

        activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);

        activity?.SetTag("react.output.message", response.Messages.First().Text);

        return new ActRequest(response.Messages.First());
    }
}