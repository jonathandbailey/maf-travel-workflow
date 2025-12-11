using System.Text;
using Application.Agents;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Nodes;

public class UserNode(IAgent agent, IAgent parsingAgent) : ReflectingExecutor<UserNode>(WorkflowConstants.UserNodeName), 
    IMessageHandler<RequestUserInput>, 
    IMessageHandler<UserResponse, ReasoningInputDto>,
    IMessageHandler<UserInput, ReasoningInputDto>
{
    public async ValueTask HandleAsync(RequestUserInput requestUserInput, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.UserNodeName}.handleRequest");

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, requestUserInput.Message);

        var stringBuilder = new StringBuilder();

        await foreach (var update in agent.RunStreamingAsync(new ChatMessage(ChatRole.User, requestUserInput.Message), cancellationToken: cancellationToken))
        {
            await context.AddEventAsync(new UserStreamingEvent(update.Text), cancellationToken);

            stringBuilder.Append(update.Text);
        }

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, stringBuilder.ToString());

        await context.AddEventAsync(new UserStreamingCompleteEvent(), cancellationToken);

        await context.SendMessageAsync(new UserRequest(stringBuilder.ToString()), cancellationToken: cancellationToken);
    }

    public async ValueTask<ReasoningInputDto> HandleAsync(UserResponse message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        return await HandleAsync(new UserInput(message.Message), context, cancellationToken);
    }

    public async ValueTask<ReasoningInputDto> HandleAsync(UserInput message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ParserNodeName}.observe");

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.InputNodePreview, message.Message);

        var response = await parsingAgent.RunAsync(new ChatMessage(ChatRole.User, message.Message), cancellationToken);

        WorkflowTelemetryTags.Preview(activity, WorkflowTelemetryTags.OutputNodePreview, response.Text);

        return new ReasoningInputDto(response.Text, "UserInput");
    }
}