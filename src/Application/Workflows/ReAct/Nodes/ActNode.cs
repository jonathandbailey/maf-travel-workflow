using System.Diagnostics;
using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace Application.Workflows.ReAct.Nodes;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>("ActNode"), IMessageHandler<ActRequest>, 
    IMessageHandler<UserResponse>

{
    private List<ChatMessage> _messages = [];

    public async ValueTask HandleAsync(ActRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start("act_handle_request");
        
        activity?.SetTag("react.node", "act_node");


        activity?.SetTag("react.input.message", request.Message.Text);

        _messages.Add(request.Message);

        activity?.AddEvent(new ActivityEvent("llm_request_sent"));

        var response = await agent.RunAsync(_messages, cancellationToken: cancellationToken);

        activity?.AddEvent(new ActivityEvent("llm_response_received"));

        _messages.Add(response.Messages.First());

        activity?.SetTag("react.output.message", response.Messages.First().Text);

        if (JsonOutputParser.HasJson(response.Text))
        {
            var routeAction = JsonOutputParser.Parse<RouteAction>(response.Text);

            activity?.SetTag("react.route", routeAction.Route);

            var cleanedResponse = JsonOutputParser.Remove(response.Text);

            if (routeAction.Route == "ask_user")
            {
                activity?.AddEvent(new ActivityEvent("react_user_request"));
                activity?.SetTag("react.user.message", cleanedResponse);

                await context.SendMessageAsync(new UserRequest(cleanedResponse), cancellationToken: cancellationToken);
            }

            if (routeAction.Route == "complete")
            {
                activity?.AddEvent(new ActivityEvent("react_complete"));
                activity?.SetTag("react.complete.result", cleanedResponse);

                await context.AddEventAsync(new ReasonActWorkflowCompleteEvent(cleanedResponse), cancellationToken);

            }
        }
        else
        {
            activity?.AddEvent(new ActivityEvent("react_no_action"));
        }
    }

    public async ValueTask HandleAsync(UserResponse userResponse, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("act_handle_user_response");

        activity?.SetTag("react.user.response_message", userResponse.Message);

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