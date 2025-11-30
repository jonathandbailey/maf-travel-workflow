using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using Application.Workflows.Events;
using Microsoft.Agents.AI;

namespace Application.Workflows.ReAct.Nodes;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), IMessageHandler<ActRequest>, 
    IMessageHandler<UserResponse>
{
    private const string NoJsonReturnedByAgent = "Agent/LLM did not return formnatted JSON for routing/actions.";
    
    private Activity? _activity;
 
    public async ValueTask HandleAsync(ActRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        TraceActRequest(request);

        var userId = await context.UserId();
        var sessionId = await context.SessionId();
    
        var response = await agent.RunAsync(new List<ChatMessage> { request.Message }, sessionId, userId, cancellationToken);

        TraceAgentRequestSent(response);

        if (!JsonOutputParser.HasJson(response.Text))
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(NoJsonReturnedByAgent,response.Text, WorkflowConstants.ActNodeName), cancellationToken);
            return;
        }
        
        var routeAction = JsonOutputParser.Parse<RouteAction>(response.Text);

        var cleanedResponse = JsonOutputParser.Remove(response.Text);

        _activity?.SetTag("react.route.message", cleanedResponse);
        _activity?.SetTag("react.route", routeAction.Route);

        switch (routeAction.Route)
        {
            case "ask_user":
                await context.SendMessageAsync(new UserRequest(cleanedResponse), cancellationToken: cancellationToken);
                break;
            case "complete":
                await context.AddEventAsync(new ReasonActWorkflowCompleteEvent(cleanedResponse), cancellationToken);
                break;
            case "orchestrate":
            {
                var extractedJson = JsonOutputParser.ExtractJson(response.Text);

                await context.SendMessageAsync(new OrchestrationRequest(extractedJson), cancellationToken: cancellationToken);
                break;
            }
        }

        TraceEnd();
    }

    public async ValueTask HandleAsync(UserResponse userResponse, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("ActHandleUserResponse");

        activity?.SetTag("react.user.response_message", userResponse.Message);

        await context.SendMessageAsync(new ActObservation(userResponse.Message), cancellationToken: cancellationToken);
    }

    private void TraceActRequest(ActRequest request)
    {
        _activity = Telemetry.Start("ActHandleRequest");

        _activity?.SetTag("react.node", "act_node");

        _activity?.SetTag("react.input.message", request.Message.Text);
    }

    private void TraceAgentRequestSent(AgentRunResponse response)
    {
        _activity?.SetTag("react.output.message", response.Messages.First().Text);
    }

    private void TraceEnd()
    {
        _activity?.Dispose();
    }
}

