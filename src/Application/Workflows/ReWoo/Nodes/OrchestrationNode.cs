using Application.Agents;
using Application.Observability;
using Application.Workflows.ReAct.Dto;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class OrchestrationNode(IAgent agent) : ReflectingExecutor<OrchestrationNode>("OrchestrationNode"), IMessageHandler<OrchestrationRequest>
{
    private List<ChatMessage> _messages = [];

    public async ValueTask HandleAsync(OrchestrationRequest message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("OrchestrationActHandleRequest");

        activity?.SetTag("re-woo.node", "orchestration_node");

        activity?.SetTag("re-woo.input.message", message.Text);

        _messages.Add(new ChatMessage(ChatRole.User, message.Text));

        activity?.AddEvent(new ActivityEvent("LLMRequestSent"));

        var userId = await context.ReadStateAsync<Guid>("UserId", scopeName: "Global", cancellationToken);
        var sessionId = await context.ReadStateAsync<Guid>("SessionId", scopeName: "Global", cancellationToken);

        var response = await agent.RunAsync(_messages, sessionId, userId, cancellationToken: cancellationToken);

        activity?.AddEvent(new ActivityEvent("LLMResponseReceived"));

        var responseMessage = response.Messages.First();

        _messages.Add(responseMessage);

        activity?.SetTag("re-woo.output.message", response.Messages.First().Text);

        var json = JsonOutputParser.ExtractJson(response.Messages.First().Text);

        var orchestrationRequest = JsonSerializer.Deserialize<OrchestrationRequestDto>(json);

        if (orchestrationRequest != null)
        {
            foreach (var orchestratorWorkerTaskDto in orchestrationRequest.Tasks)
            {
                await context.SendMessageAsync(orchestratorWorkerTaskDto, cancellationToken: cancellationToken);
            }
        }
    }
}