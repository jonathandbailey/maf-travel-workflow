using Application.Agents;
using Application.Observability;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations.Nodes;

public class ReasonNode(IAgent agent) : ReflectingExecutor<ReasonNode>("ReasonNode") , IMessageHandler<ChatMessage, ActRequest>,
    IMessageHandler<ActObservation, ChatMessage>
{
    public async ValueTask<ActRequest> HandleAsync(ChatMessage message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.StarActivity("Reason-Node");

        activity?.SetTag("User", message.Text);

        var response = await agent.RunAsync(new List<ChatMessage> { message }, cancellationToken: cancellationToken);

        activity?.SetTag("Assistant", response.Messages.First().Text);

        return new ActRequest(response.Messages.First());
    }

    public async ValueTask<ChatMessage> HandleAsync(ActObservation actObservation, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var requestMessage = new ChatMessage(ChatRole.User, actObservation.Message);

        var response = await agent.RunAsync(new List<ChatMessage> { requestMessage }, cancellationToken: cancellationToken);

        return response.Messages.First();
    }
}