using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using Microsoft.Agents.AI;

namespace Application.Workflows.ReAct.Nodes;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), IMessageHandler<ActRequest>, 
    IMessageHandler<UserResponse>
{
    private Activity? _activity;
 
    public async ValueTask HandleAsync(ActRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        TraceActRequest(request);

        var userId = await context.ReadStateAsync<Guid>("UserId", scopeName:"Global", cancellationToken);
        var sessionId = await context.ReadStateAsync<Guid>("SessionId", scopeName:"Global", cancellationToken);
    
        var response = await agent.RunAsync(new List<ChatMessage> { request.Message }, sessionId, userId, cancellationToken);

        TraceAgentRequestSent(response);

        if (!JsonOutputParser.HasJson(response.Text))
        {
            await context.AddEventAsync(new WorkflowErrorEvent("Invalid JSON response", "Act Node"), cancellationToken);
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
        _activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        _activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        _activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);

        _activity?.SetTag("react.output.message", response.Messages.First().Text);
    }

    private void TraceEnd()
    {
        _activity?.Dispose();
    }
}



internal sealed class ReasonActWorkflowCompleteEvent(string message) : WorkflowEvent(message)
{
    public string Message { get; } = message;
}

public sealed class WorkflowErrorEvent(string message, string nodeName) : WorkflowEvent(message)
{
    public string Message { get; } = message;
    public string NodeName { get; } = nodeName;
}
