using Application.Agents;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class OrchestrationNode(IAgent agent) : ReflectingExecutor<OrchestrationNode>(WorkflowConstants.OrchestrationNodeName), IMessageHandler<OrchestrationRequest>
{
    private const string OrchestrationNodeError = "Orchestration Node has failed to execute.";
    private const string StatusBuildingTravelPlan = "Building Travel Plan...";


    public async ValueTask HandleAsync(OrchestrationRequest message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"{WorkflowConstants.OrchestrationNodeName}.handleRequest");

        activity?.SetTag(WorkflowTelemetryTags.Node, WorkflowConstants.OrchestrationNodeName);

        WorkflowTelemetryTags.SetInputPreview(activity, message.Text);

        try
        {
            await context.AddEventAsync(new WorkflowStatusEvent(StatusBuildingTravelPlan), cancellationToken);

            var userId = await context.UserId();
            var sessionId = await context.SessionId();

            var response = await agent.RunAsync(new ChatMessage(ChatRole.User, message.Text), sessionId, userId, cancellationToken: cancellationToken);

            WorkflowTelemetryTags.SetOutputPreview(activity, response.Text);

            var json = JsonOutputParser.ExtractJson(response.Text);

            var orchestrationRequest = JsonSerializer.Deserialize<OrchestrationRequestDto>(json);

            if (orchestrationRequest != null)
            {
                foreach (var orchestratorWorkerTaskDto in orchestrationRequest.Tasks)
                {
                    await context.SendMessageAsync(orchestratorWorkerTaskDto, cancellationToken: cancellationToken);
                }
            }
        }
        catch (Exception exception)
        {
            await context.AddEventAsync(new TravelWorkflowErrorEvent(OrchestrationNodeError, OrchestrationNodeError, WorkflowConstants.OrchestrationNodeName, exception), cancellationToken);
        }
    }
}