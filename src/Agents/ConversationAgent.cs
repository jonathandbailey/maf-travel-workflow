using A2A;
using Agents.Extensions;
using Agents.Observability;
using Agents.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.Logging;

namespace Agents;

public class ConversationAgent(AIAgent agent, IA2AAgentServiceDiscovery discovery, ILogger<IAgentFactory> logger) : DelegatingAIAgent(agent)
{
    private const string StatusMessageThinking = "Thinking...";
    private const string ExecutingTravelWorkflow = "Executing Travel Workflow...";
    private const string ProcessingResults = "Processing Results...";
    
    protected override async IAsyncEnumerable<AgentRunResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var localMessages = messages.ToList();

        Verify.NotNull(options);
        Verify.NotNull(thread);
        Verify.NotEmpty(localMessages);
      
        var threadId = options.GetAgUiThreadId();

        options = options.AddThreadId(threadId);

        using var activity = UserAgentTelemetry.Start(localMessages.First().Text, threadId);

        yield return StatusMessageThinking.ToAgentResponseStatusMessage();

        
        var tools = new Dictionary<string, FunctionCallContent>();

        await foreach (var update in InnerAgent.RunStreamingAsync(localMessages, thread, options, cancellationToken))
        {
            tools.AddToolCalls(update.Contents);

            yield return update;
        }
        
     
        yield return ExecutingTravelWorkflow.ToAgentResponseStatusMessage();


        var toolResults = new List<AIContent>();

        foreach (var functionCallContent in tools)
        {
            var arguments = discovery.GetToolCallArguments(functionCallContent.Value);

            var toolActivity = UserAgentTelemetry.StartTool(functionCallContent.Key, arguments);
       
            var agentMeta = discovery.GetAgentMeta(functionCallContent.Key);

            var agentThread = agentMeta.Agent.GetNewThread(threadId);

            var ex = new A2AAgentEx(agentMeta.Agent);

            await foreach (var agentRunUpdate in ex.RunCoreStreamingAsync(agentMeta.Client,
                               new List<ChatMessage> { new(ChatRole.User, arguments) },(A2AAgentThread) agentThread,
                               cancellationToken: cancellationToken))
            {
       
                if (agentRunUpdate.RawRepresentation is TaskArtifactUpdateEvent artifactUpdateEvent)
                {
                    var messageText = artifactUpdateEvent.GetPartArtifactDataText();

                    toolActivity?.AddEvent(agentRunUpdate, messageText.Key);

                    yield return messageText.ToAgentResponseStatusMessage();
                }

                if (agentRunUpdate.RawRepresentation is TaskStatusUpdateEvent taskStatusUpdateEvent)
                {
                    if (taskStatusUpdateEvent.Status.State is TaskState.InputRequired or TaskState.Completed)
                    {
                        var content = taskStatusUpdateEvent.GetPartText();

                        toolActivity?.AddEvent(agentRunUpdate, taskStatusUpdateEvent.Status.State, content);

                        toolResults.Add(new FunctionResultContent(
                            functionCallContent.Value.CallId,
                            content));
                    }

                    if (taskStatusUpdateEvent.Status.State == TaskState.Working)
                    {
                        var messageText = taskStatusUpdateEvent.GetPartStatusDataText();

                        toolActivity?.AddEvent(agentRunUpdate, taskStatusUpdateEvent.Status.State, messageText.Status);

                        yield return messageText.ToAgentResponseStatusMessage();
                    }
                }
            }

            toolActivity?.Dispose();
         
        }
   
        yield return ProcessingResults.ToAgentResponseStatusMessage();

        await foreach (var update in InnerAgent.RunStreamingAsync([new ChatMessage(ChatRole.Tool, toolResults)], thread, cancellationToken: cancellationToken))
        {
            yield return update;
        }
    }
}