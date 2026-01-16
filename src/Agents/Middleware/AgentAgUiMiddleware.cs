using Agents.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace Agents.Middleware;

public class AgentAgUiMiddleware : IAgentAgUiMiddleware
{
    public async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var threadId = options?.GetAgUiThreadId();

        options ??= new AgentRunOptions();

        options = options.AddThreadId(threadId!);

        await foreach (var update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            yield return update;
        }
    }
}

public interface IAgentAgUiMiddleware
{
    IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread,
        AgentRunOptions? options,
        AIAgent innerAgent,
        CancellationToken cancellationToken);
}