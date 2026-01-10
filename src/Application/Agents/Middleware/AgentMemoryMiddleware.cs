using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Application.Services;

namespace Application.Agents.Middleware;


public class AgentMemoryMiddleware(IAgentMemoryService memory) : IAgentMemoryMiddleware
{
    public async Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        var memoryThread = await LoadAsync(innerAgent);

        var response = await innerAgent.RunAsync(messages, memoryThread, options, cancellationToken);

        var threadState = memoryThread.Serialize();

        await memory.SaveAsync(new AgentState(threadState), innerAgent.Name!);


        return response;
    }

    public async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var memoryThread = await LoadAsync(innerAgent);
        
        await foreach (var update in innerAgent.RunStreamingAsync(messages, memoryThread, options, cancellationToken))
        {
            yield return update;
        }

        var threadState = memoryThread.Serialize();

        await memory.SaveAsync(new AgentState(threadState), innerAgent.Name!);
    }

    private async Task<AgentThread> LoadAsync(AIAgent agent)
    {
        AgentThread? thread;

        if (!await memory.ExistsAsync(agent.Name!))
        {
            thread = agent.GetNewThread();
        }
        else
        {
            var stateDto = await memory.LoadAsync(agent.Name!);

            thread = agent.DeserializeThread(stateDto.Thread);
        }

        return thread;
    }
}

public interface IAgentMemoryMiddleware
{
    IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);

    Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);
}