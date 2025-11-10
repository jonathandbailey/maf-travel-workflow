using Application.Agents;
using Application.Observability;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations.Nodes;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>("ActNode"), IMessageHandler<ActRequest>, 
    IMessageHandler<UserResponse>

{
    private List<ChatMessage> _messages = [];

    public async ValueTask HandleAsync(ActRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Trace("Act-[handle]");

        activity?.SetTag("Reason Request (Act Node)", request.Message);

        _messages.Add(request.Message);

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        _messages.Add(response.Messages.First());

        activity?.SetTag("Act-[response]", response.Messages.First().Text);

        if (JsonOutputParser.HasJson(response.Text))
        {
            var routeAction = JsonOutputParser.Parse<RouteAction>(response.Text);

            if (routeAction.Route == "ask_user")
            {
                var cleanedResponse = JsonOutputParser.Remove(response.Text);

                using var askUserActivity = Telemetry.Trace("Act-[user-request]");

                askUserActivity?.SetTag("RequestUser:", cleanedResponse);

                await context.SendMessageAsync(new UserRequest(cleanedResponse), cancellationToken: cancellationToken);
            }

            if (routeAction.Route == "complete")
            {
                var cleanedResponse = JsonOutputParser.Remove(response.Text);

                using var askUserActivity = Telemetry.Trace("Act-[complete]");

                askUserActivity?.SetTag("Response:", cleanedResponse);

                await context.AddEventAsync(new ReasonActWorkflowCompleteEvent(cleanedResponse), cancellationToken);

            }
        }
        else
        {
            using var askUserActivity = Telemetry.Trace("Act-[no-action]");
        }
    }

    public async ValueTask HandleAsync(UserResponse userResponse, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Trace("Act-[user-response]");

        activity?.SetTag("User Response/Observation:", userResponse);

        await context.SendMessageAsync(new ActObservation(userResponse.Message), cancellationToken: cancellationToken);
    }

    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return context.QueueStateUpdateAsync("act-node-messages", _messages, cancellationToken: cancellationToken);
    }

    protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _messages = (await context.ReadStateAsync<List<ChatMessage>>("act-node-messages", cancellationToken: cancellationToken))!;
    }
}

internal sealed class ReasonActWorkflowCompleteEvent(string message) : WorkflowEvent(message)
{
    public string Message { get; } = message;
}