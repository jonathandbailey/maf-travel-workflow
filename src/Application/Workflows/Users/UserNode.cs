using System.Text;
using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Users;

public class UserNode(IAgent agent) : ReflectingExecutor<UserNode>(WorkflowConstants.UserNodeName), 
    IMessageHandler<ActUserRequest>, 
    IMessageHandler<UserResponse>
{
    public async ValueTask HandleAsync(ActUserRequest actUserRequest, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.UserNodeName}.handleRequest");

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, actUserRequest.Message);

        var stringBuilder = new StringBuilder();

        await foreach (var update in agent.RunStreamingAsync(new ChatMessage(ChatRole.User, actUserRequest.Message), cancellationToken: cancellationToken))
        {
            await context.AddEventAsync(new UserStreamingEvent(update.Text), cancellationToken);

            stringBuilder.Append(update.Text);
        }

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, stringBuilder.ToString());

        await context.AddEventAsync(new UserStreamingCompleteEvent(), cancellationToken);

        await context.SendMessageAsync(new UserRequest(stringBuilder.ToString()), cancellationToken: cancellationToken);
    }

    public async ValueTask HandleAsync(UserResponse message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.SendMessageAsync(new ActObservation(message.Message, "UserInput"), cancellationToken: cancellationToken);
    }
}