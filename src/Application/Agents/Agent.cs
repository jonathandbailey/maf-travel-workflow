using Application.Infrastructure;
using Application.Observability;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Application.Agents;

public class Agent(AIAgent agent, IAgentThreadRepository repository, AgentTypes type) : IAgent
{
    private const string AgentTelemetryName = "Agent";
    public async Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start(AgentTelemetryName);

        var thread = await LoadAsync(userId, sessionId, type);

        var response =  await agent.RunAsync(messages, thread, cancellationToken: cancellationToken);

        TraceTokenUsage(response, activity);
        
        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());

        return response;
    }

    public async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        ChatMessage message,
        Guid sessionId,
        Guid userId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var thread = await LoadAsync(userId, sessionId, type);

        await foreach (var update in agent.RunStreamingAsync(message, thread, cancellationToken: cancellationToken))
        {
           yield return update;
        }

        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());
    }

    public async Task<AgentRunResponse> RunAsync(
        ChatMessage message,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start(AgentTelemetryName);

        var thread = await LoadAsync(userId, sessionId, type);

        var response = await agent.RunAsync(message, thread, cancellationToken: cancellationToken);

        TraceTokenUsage(response, activity);

        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());

        return response;
    }

    private static void TraceTokenUsage(AgentRunResponse response, Activity? activity)
    {
        activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);
    }

    private async Task<AgentThread> LoadAsync(Guid userId, Guid sessionId, AgentTypes agentType)
    {
        AgentThread? thread;

        if (!await repository.ExistsAsync(userId, sessionId, agentType.ToString()))
        {
            thread = agent.GetNewThread();
        }
        else
        {
            var stateDto = await repository.LoadAsync(userId, sessionId, agentType.ToString());

            thread = agent.DeserializeThread(stateDto.Thread);
        }

        return thread;
    }
}
public interface IAgent
{
    Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, Guid sessionId, Guid userId, CancellationToken cancellationToken = default);

    Task<AgentRunResponse> RunAsync(
        ChatMessage message,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        ChatMessage message,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default);
}