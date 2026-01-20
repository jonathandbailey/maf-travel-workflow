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

public class UserAgent(AIAgent agent, IA2AAgentServiceDiscovery discovery, ILogger<IAgentFactory> logger) : DelegatingAIAgent(agent)
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

        if (localMessages.Count == 0)
        {
            logger.LogError("User Agent was called with no messages");
            throw new Exception("No messages provided");
        }

        if (options == null)
        {
            logger.LogError("User Agent was called with no Agent Run Options");
            throw new Exception("No Agent Run Options provided");
        }
        
        
        using var activity = Telemetry.Start($"UserAgent.Run");
        activity?.SetTag("Input", localMessages.First().Text);


        var threadId = options.GetAgUiThreadId();

        options = options.AddThreadId(threadId);
    
        yield return StatusMessageThinking.ToAgentResponseStatusMessage();

        var tools = new Dictionary<string, FunctionCallContent>();

        await foreach (var update in InnerAgent.RunStreamingAsync(localMessages, thread, options, cancellationToken))
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
     
        yield return ExecutingTravelWorkflow.ToAgentResponseStatusMessage();

        foreach (var functionCallContent in tools)
        {
            var toolActivity = Telemetry.Start($"UserAgent.Tool.{functionCallContent.Key}");
         
            

            var agentMeta = discovery.GetAgentMeta(functionCallContent.Key);

            var agentThread = agentMeta.Agent.GetNewThread(threadId!);

            var argument = functionCallContent.Value.Arguments["jsonPayload"].ToString();

            toolActivity?.SetTag("Tool Call Arguments", argument);


            var ex = new A2AAgentEx(agentMeta.Agent);

            await foreach (var agentRunUpdate in ex.RunCoreStreamingAsync(agentMeta.Client,
                               new List<ChatMessage>() { new ChatMessage(ChatRole.User, argument) },(A2AAgentThread) agentThread,
                               cancellationToken: cancellationToken))
            {
                toolActivity?.AddEvent(new System.Diagnostics.ActivityEvent("AgentRunUpdate", 
                    tags: new System.Diagnostics.ActivityTagsCollection
                    {
                        { "UpdateType", agentRunUpdate.RawRepresentation?.GetType().Name ?? "Unknown" }
                    }));

                if (agentRunUpdate.RawRepresentation is TaskArtifactUpdateEvent)
                {
                    var artifactEvent = agentRunUpdate.RawRepresentation as TaskArtifactUpdateEvent;
                    var messageText = artifactEvent.Artifact.Parts.OfType<TextPart>().First().Text;

                    toolActivity?.AddEvent(new System.Diagnostics.ActivityEvent("TaskArtifact", 
                        tags: new System.Diagnostics.ActivityTagsCollection
                        {
                            { "MessageText", messageText }
                        }));
                }

                if (agentRunUpdate.RawRepresentation is TaskStatusUpdateEvent)
                {
                    var message = agentRunUpdate.RawRepresentation as TaskStatusUpdateEvent;
                    var messageText = message.Status.Message.Parts.OfType<TextPart>().First().Text;

                    toolActivity?.AddEvent(new System.Diagnostics.ActivityEvent("TaskStatus", 
                        tags: new System.Diagnostics.ActivityTagsCollection
                        {
                            { "State", message.Status.State.ToString() },
                            { "MessageText", messageText }
                        }));

                    if (message.Status.State == TaskState.InputRequired)
                    {
                        toolResults.Add(new FunctionResultContent(
                            functionCallContent.Value.CallId,
                            messageText));
                    }

                    if (message.Status.State == TaskState.Completed)
                    {
                        toolResults.Add(new FunctionResultContent(
                            functionCallContent.Value.CallId,
                            messageText));
                    }

                    if (message.Status.State == TaskState.Working)
                    {
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