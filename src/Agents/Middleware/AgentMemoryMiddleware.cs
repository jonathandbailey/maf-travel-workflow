using System.Runtime.CompilerServices;
using Agents.Extensions;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Agents.Middleware;

public class AgentMemoryMiddleware(IAgentMemoryService memory) : IAgentMemoryMiddleware
{
    public async Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken)
    {
        var threadId = options.GetThreadId();

        var memoryThread = await LoadAsync(innerAgent, threadId);

        var response = await innerAgent.RunAsync(messages, memoryThread, options, cancellationToken);

        var threadState = memoryThread.Serialize();

        await memory.SaveAsync(new AgentState(threadState), GetResourceName(innerAgent.Name!, threadId));


        return response;
    }

    public async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var threadId = options.GetThreadId();
        
        var memoryThread = await LoadAsync(innerAgent, threadId);
        
        await foreach (var update in innerAgent.RunStreamingAsync(messages, memoryThread, options, cancellationToken))
        {
            yield return update;
        }

        var threadState = memoryThread.Serialize();

        await memory.SaveAsync(new AgentState(threadState), GetResourceName(innerAgent.Name!, threadId));
    }

    private async Task<AgentThread> LoadAsync(AIAgent agent, string threadId)
    {
        AgentThread? thread;

        if (!await memory.ExistsAsync(GetResourceName(agent.Name!, threadId)))
        {
            thread = agent.GetNewThread();
        }
        else
        {
            var stateDto = await memory.LoadAsync(GetResourceName(agent.Name!, threadId));

            thread = agent.DeserializeThread(stateDto.Thread);
        }

        return thread;
    }

    private static string GetResourceName(string agentName, string threadId)
    {
        return $"{agentName}_{threadId}";
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