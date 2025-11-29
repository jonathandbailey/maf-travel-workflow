using Application.Agents;
using Application.Observability;
using Application.Workflows.ReWoo.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Workflows.ReWoo.Nodes;

public class HotelWorkerNode(IAgent agent) : ReflectingExecutor<HotelWorkerNode>("HotelWorkerNode"), IMessageHandler<OrchestratorWorkerTaskDto>
{
    private List<ChatMessage> _messages = [];

    public async ValueTask HandleAsync(OrchestratorWorkerTaskDto message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        using var activity = Telemetry.Start("HotelWorkerHandleRequest");

        activity?.SetTag("re-woo.node", "hotel_worker_node");

        activity?.SetTag("re-woo.input.message", message);
  
        var serialized = JsonSerializer.Serialize(message);

        activity?.SetTag("re-woo.input.message", serialized);

        _messages.Add(new ChatMessage(ChatRole.User, serialized));

        activity?.AddEvent(new ActivityEvent("LLMRequestSent"));

        var userId = await context.UserId();
        var sessionId = await context.SessionId();

        var response = await agent.RunAsync(new List<ChatMessage> { new(ChatRole.User, serialized) }, sessionId, userId, cancellationToken: cancellationToken);

        activity?.AddEvent(new ActivityEvent("LLMResponseReceived"));

        var responseMessage = response.Messages.First();

        activity?.SetTag("re-woo.output.message", response.Messages.First().Text);

        await context.SendMessageAsync(new ArtifactStorageDto(message.ArtifactKey, responseMessage.Text), cancellationToken: cancellationToken);
    }
}