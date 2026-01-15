using Agents.Observability;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Agents.Extensions;
using Agents.Services;

namespace Agents;

public class UserAgent(AIAgent agent, IA2AAgentServiceDiscovery discovery) : DelegatingAIAgent(agent)
{
    protected override async IAsyncEnumerable<AgentRunResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var activity = Telemetry.Start($"UserAgent.Run");
        activity?.SetTag("Input", messages);


        var threadId = options?.GetAgUiThreadId();

        var tools = new Dictionary<string, FunctionCallContent>();

        await foreach (var update in InnerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            foreach (var content in update.Contents)
            {
                if (content is FunctionCallContent callContent)
                {
                    tools[callContent.Name] = callContent;
                }
            }

            yield return update;
        }

        var toolMessage = new List<ChatMessage>();

        foreach (var functionCallContent in tools)
        {
            var agentMeta = discovery.GetAgentMeta(functionCallContent.Key);

            var agentThread = agentMeta.Agent.GetNewThread(threadId!);

            var argument = functionCallContent.Value.Arguments["jsonPayload"].ToString();

            var toolResponse = await agentMeta.Agent.RunAsync(new ChatMessage(ChatRole.User, argument), agentThread, cancellationToken: cancellationToken);

            toolMessage.AddRange(toolResponse.Messages);

        }

        await foreach (var update in InnerAgent.RunStreamingAsync(toolMessage, thread, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }
}