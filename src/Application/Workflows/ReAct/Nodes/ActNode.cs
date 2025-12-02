using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text;

namespace Application.Workflows.ReAct.Nodes;

public class ActNode(IAgent agent) : ReflectingExecutor<ActNode>(WorkflowConstants.ActNodeName), IMessageHandler<ActRequest>
{
    private const string NoJsonReturnedByAgent = "Agent/LLM did not return formnatted JSON for routing/actions";
    private const string AgentJsonParseFailed = "Agent JSON parse failed";

    public async ValueTask HandleAsync(ActRequest request, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.ActNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.ActNodeName);

        WorkflowTelemetryTags.SetInputPreview(activity, request.Message.Text);

        var userId = await context.UserId();
        var sessionId = await context.SessionId();
    
        var stringBuilder = new StringBuilder();

        await foreach (var update in agent.RunStreamingAsync(request.Message, sessionId, userId, cancellationToken: cancellationToken))
        {
            stringBuilder.Append(update.Text);
            await context.AddEventAsync(new ConversationStreamingEvent(update.Text), cancellationToken);
        }

        var response = stringBuilder.ToString();

        WorkflowTelemetryTags.SetInputPreview(activity, response);

        if (!JsonOutputParser.HasJson(response))
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(NoJsonReturnedByAgent,response, WorkflowConstants.ActNodeName), cancellationToken);
            return;
        }
        
        RouteAction routeAction;
        
        try
        {
            routeAction = JsonOutputParser.Parse<RouteAction>(response);
        }
        catch (Exception ex)
        {
            await context.AddEventAsync(
                new TravelWorkflowErrorEvent(AgentJsonParseFailed, response, WorkflowConstants.ActNodeName, ex),
                cancellationToken);
            return;
        }

        var cleanedResponse = JsonOutputParser.Remove(response);

        activity?.SetTag("workflow.route.message", cleanedResponse);
        activity?.SetTag("workflow.route", routeAction.Route);

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
                var extractedJson = JsonOutputParser.ExtractJson(response);

                await context.SendMessageAsync(new OrchestrationRequest(extractedJson), cancellationToken: cancellationToken);
                break;
            }
            default:
                await context.AddEventAsync(
                    new TravelWorkflowErrorEvent($"Unknown route '{routeAction.Route}' returned by agent", cleanedResponse, WorkflowConstants.ActNodeName),
                    cancellationToken);
                break;
        }
    }
}

