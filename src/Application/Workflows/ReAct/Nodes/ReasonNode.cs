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
    private List<ChatMessage> _messages = [];
    
    public async ValueTask<ActRequest> HandleAsync(TravelWorkflowRequestDto requestDto, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start("ReasonHandleRequest");

        activity?.SetTag("react.node", "reason_node");

        activity?.SetTag("react.input.message", requestDto.Message.Text);

        await context.QueueStateUpdateAsync("SessionId", requestDto.SessionId, scopeName:"Global", cancellationToken);
        await context.QueueStateUpdateAsync("UserId", requestDto.UserId, scopeName:"Global", cancellationToken);

        return await Process(requestDto.Message, activity, cancellationToken);
    }

    public async ValueTask<ActRequest> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("ReasonHandleObserve");

        activity?.SetTag("react.node", "reason_node");

        activity?.SetTag("react.observation.response_message", actObservation.Message);

        var message = new ChatMessage(ChatRole.User, actObservation.Message);

        return await Process(message, activity, cancellationToken);
    }

    private async Task<ActRequest> Process(ChatMessage message, Activity? activity, CancellationToken cancellationToken)
    {
        _messages.Add(message);

        activity?.AddEvent(new ActivityEvent("LLMRequestSent"));

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        activity?.AddEvent(new ActivityEvent("LLMResponseReceived"));

        var responseMessage = response.Messages.First();

        activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);

        _messages.Add(responseMessage);

        activity?.SetTag("react.output.message", response.Messages.First().Text);

        return new ActRequest(response.Messages.First());
    }

    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return context.QueueStateUpdateAsync("reason-node-messages", _messages, cancellationToken: cancellationToken);
    }

    protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _messages = (await context.ReadStateAsync<List<ChatMessage>>("reason-node-messages", cancellationToken: cancellationToken))!;
    }
}