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
        activity?.SetTag("Input", messages.First().Text);


        var threadId = options?.GetAgUiThreadId();

        options = options.AddThreadId(threadId!);

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

        var toolResults = new List<AIContent>();

        foreach (var functionCallContent in tools)
        {
            var agentMeta = discovery.GetAgentMeta(functionCallContent.Key);

            var agentThread = agentMeta.Agent.GetNewThread(threadId!);

            var argument = functionCallContent.Value.Arguments["jsonPayload"].ToString();

            var toolResponse = await agentMeta.Agent.RunAsync(new ChatMessage(ChatRole.User, argument), agentThread, cancellationToken: cancellationToken);

            var resultContent = new FunctionResultContent(
                functionCallContent.Value.CallId,
                toolResponse.Messages.First().Text);
            
            toolResults.Add(resultContent);

            activity?.SetTag("Tool Response", toolResponse.Messages.First().Text);

        }

        await foreach (var update in InnerAgent.RunStreamingAsync([new ChatMessage(ChatRole.Tool, toolResults)], thread, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }
}