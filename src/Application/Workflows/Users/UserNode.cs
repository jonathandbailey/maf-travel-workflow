using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace Application.Workflows.Users;

public class UserNode() : ReflectingExecutor<UserNode>(WorkflowConstants.UserNode), IMessageHandler<ActUserRequest>
{
    public async ValueTask HandleAsync(ActUserRequest actUserRequest, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.SendMessageAsync(new UserRequest(actUserRequest.Message), cancellationToken: cancellationToken);
    }
}