using Application.Agents;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Users;

public class UserNode(IAgent agent) : ReflectingExecutor<UserNode>(WorkflowConstants.UserNode), IMessageHandler<ActUserRequest>
{
    public async ValueTask HandleAsync(ActUserRequest actUserRequest, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var userId = await context.UserId();
        var sessionId = await context.SessionId();
     
        await foreach (var update in agent.RunStreamingAsync(new ChatMessage(ChatRole.User, actUserRequest.Message), sessionId, userId, cancellationToken: cancellationToken))
        {
            await context.AddEventAsync(new ConversationStreamingEvent(update.Text, false), cancellationToken);
        }

        await context.AddEventAsync(new ConversationStreamingEvent(string.Empty, true), cancellationToken);

        await context.SendMessageAsync(new UserRequest(actUserRequest.Message), cancellationToken: cancellationToken);
    }
}