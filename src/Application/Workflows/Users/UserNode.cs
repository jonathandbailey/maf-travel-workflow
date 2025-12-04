using Application.Agents;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Users;

public class UserNode(IAgent agent) : ReflectingExecutor<UserNode>(WorkflowConstants.UserNode), 
    IMessageHandler<ActUserRequest>, 
    IMessageHandler<UserResponse>
{
    public async ValueTask HandleAsync(ActUserRequest actUserRequest, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await foreach (var update in agent.RunStreamingAsync(new ChatMessage(ChatRole.User, actUserRequest.Message), cancellationToken: cancellationToken))
        {
            await context.AddEventAsync(new UserStreamingEvent(update.Text), cancellationToken);
        }

        await context.AddEventAsync(new UserStreamingCompleteEvent(), cancellationToken);

        await context.SendMessageAsync(new UserRequest(actUserRequest.Message), cancellationToken: cancellationToken);
    }

    public async ValueTask HandleAsync(UserResponse message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.SendMessageAsync(new ActObservation(message.Message), cancellationToken: cancellationToken);
    }
}