using Application.Agents;
using Application.Observability;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations.Nodes;

public class ReasonNode(IAgent agent) : ReflectingExecutor<ReasonNode>("ReasonNode") , IMessageHandler<ChatMessage, ActRequest>,
    IMessageHandler<ActObservation, ActRequest>
{
    private List<ChatMessage> _messages = [];
    
    public async ValueTask<ActRequest> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.StarActivity("Reason-[handle]");

        activity?.SetTag("User", message.Text);

        _messages.Add(message);
        
        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        var responseMessage = response.Messages.First();
        
        _messages.Add(responseMessage);

        activity?.SetTag("Assistant", response.Messages.First().Text);

        return new ActRequest(response.Messages.First());
    }

    public async ValueTask<ActRequest> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.StarActivity("Reason-[observe]");

        activity?.SetTag("User", actObservation.Message);

        var message = new ChatMessage(ChatRole.User, actObservation.Message);

        _messages.Add(message);

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        activity?.SetTag("Assistant", response.Messages.First().Text);

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