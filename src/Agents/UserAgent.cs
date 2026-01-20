using A2A;
using Agents.Extensions;
using Agents.Observability;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI.A2A;

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

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes("Thinking...");

        yield return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, "application/json")]
        };

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

        stateBytes = JsonSerializer.SerializeToUtf8Bytes("Executing Travel Workflow...");

        yield return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, "application/json")]
        };

        foreach (var functionCallContent in tools)
        {
            var agentMeta = discovery.GetAgentMeta(functionCallContent.Key);

            var agentThread = agentMeta.Agent.GetNewThread(threadId!);

            var argument = functionCallContent.Value.Arguments["jsonPayload"].ToString();


            var ex = new A2AAgentEx(agentMeta.Agent);

            await foreach (var agentRunUpdate in ex.RunCoreStreamingAsync(agentMeta.Client,
                               new List<ChatMessage>() { new ChatMessage(ChatRole.User, argument) },(A2AAgentThread) agentThread,
                               cancellationToken: cancellationToken))
            {
                if (agentRunUpdate.RawRepresentation is TaskArtifactUpdateEvent)
                {
                    var artifactEvent = agentRunUpdate.RawRepresentation as TaskArtifactUpdateEvent;
                    var messageText = artifactEvent.Artifact.Parts.OfType<TextPart>().First().Text;

                    toolResults.Add(new FunctionResultContent(
                        functionCallContent.Value.CallId,
                        messageText));
                }

                if (agentRunUpdate.RawRepresentation is TaskStatusUpdateEvent)
                {
                    var message = agentRunUpdate.RawRepresentation as TaskStatusUpdateEvent;
                    var messageText = message.Status.Message.Parts.OfType<TextPart>().First().Text;

                    if (message.Status.State == TaskState.InputRequired)
                    {
                        toolResults.Add(new FunctionResultContent(
                            functionCallContent.Value.CallId,
                            messageText));
                    }

                    if (message.Status.State == TaskState.Working)
                    {
                        stateBytes = JsonSerializer.SerializeToUtf8Bytes(messageText);

                        yield return new AgentRunResponseUpdate
                        {
                            Contents = [new DataContent(stateBytes, "application/json")]
                        };
                    }
                }
            }
         
        }

        stateBytes = JsonSerializer.SerializeToUtf8Bytes("Processing Results...");

        yield return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, "application/json")]
        };

        await foreach (var update in InnerAgent.RunStreamingAsync([new ChatMessage(ChatRole.Tool, toolResults)], thread, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }
}