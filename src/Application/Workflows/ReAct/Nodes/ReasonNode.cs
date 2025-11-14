using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace Application.Workflows.ReAct.Nodes;

public class ReasonNode(IAgent agent) : ReflectingExecutor<ReasonNode>("ReasonNode") , IMessageHandler<ChatMessage, ActRequest>,
    IMessageHandler<ActObservation, ActRequest>
{
    private List<ChatMessage> _messages = [];
    
    public async ValueTask<ActRequest> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start("reason_handle_request");

        activity?.SetTag("react.node", "reason_node");


        activity?.SetTag("react.input.message", message.Text);

        _messages.Add(message);

        activity?.AddEvent(new ActivityEvent("llm_request_sent"));

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        activity?.AddEvent(new ActivityEvent("llm_response_received"));


        var responseMessage = response.Messages.First();
        
        _messages.Add(responseMessage);

        activity?.SetTag("react.output.message", response.Messages.First().Text);
  
        return new ActRequest(response.Messages.First());
    }

    public async ValueTask<ActRequest> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("reason_handle_observe");

        activity?.SetTag("react.observation.response_message", actObservation.Message);

        var message = new ChatMessage(ChatRole.User, actObservation.Message);

        _messages.Add(message);

        activity?.AddEvent(new ActivityEvent("llm_request_sent"));

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        _messages.Add(response.Messages.First());

        activity?.AddEvent(new ActivityEvent("llm_response_received"));

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