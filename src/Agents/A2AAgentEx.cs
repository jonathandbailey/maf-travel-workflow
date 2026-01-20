using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.AI;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Agents;

public class A2AAgentEx(AIAgent agent)
{
    public async IAsyncEnumerable<AgentRunResponseUpdate> RunCoreStreamingAsync(
        A2AClient client,
        IEnumerable<ChatMessage> messages, 
        A2AAgentThread thread, 
        AgentRunOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        A2AAgentThread typedThread = thread;

      
        ConfiguredCancelableAsyncEnumerable<SseItem<A2AEvent>> a2aSseEvents;

        MessageSendParams sendParams = new()
        {
            Message = CreateA2AMessage(typedThread, messages),
        };

        a2aSseEvents = client.SendMessageStreamingAsync(sendParams, cancellationToken).ConfigureAwait(false);

        string? contextId = null;
        string? taskId = null;

        await foreach (var sseEvent in a2aSseEvents)
        {
            if (sseEvent.Data is AgentMessage message)
            {
                contextId = message.ContextId;

                yield return this.ConvertToAgentResponseUpdate(message);
            }
            else if (sseEvent.Data is AgentTask task)
            {
                contextId = task.ContextId;
                taskId = task.Id;

                yield return this.ConvertToAgentResponseUpdate(task);
            }
            else if (sseEvent.Data is TaskUpdateEvent taskUpdateEvent)
            {
                contextId = taskUpdateEvent.ContextId;
                taskId = taskUpdateEvent.TaskId;

                yield return this.ConvertToAgentResponseUpdate(taskUpdateEvent);
            }
            else
            {
                throw new NotSupportedException($"Only message, task, task update events are supported from A2A agents. Received: {sseEvent.Data.GetType().FullName ?? "null"}");
            }
        }

        UpdateThread(typedThread, contextId, taskId);
    }

    private static void UpdateThread(A2AAgentThread? thread, string? contextId, string? taskId = null)
    {
        if (thread is null)
        {
            return;
        }

        // Surface cases where the A2A agent responds with a response that
        // has a different context Id than the thread's conversation Id.
        if (thread.ContextId is not null && !string.IsNullOrEmpty(contextId) && thread.ContextId != contextId)
        {
            throw new InvalidOperationException(
                $"The {nameof(contextId)} returned from the A2A agent is different from the conversation Id of the provided {nameof(AgentThread)}.");
        }

    
    }

    private AgentRunResponseUpdate ConvertToAgentResponseUpdate(AgentMessage message)
    {
        return new AgentRunResponseUpdate
        {
            AgentId = agent.Id,
            ResponseId = message.MessageId,
            RawRepresentation = message,
            Role = ChatRole.Assistant,
            MessageId = message.MessageId,
            Contents = message.Parts.ConvertAll(part => part.ToAIContent()),
            AdditionalProperties = message.Metadata?.ToAdditionalProperties(),
        };
    }

    private AgentRunResponseUpdate ConvertToAgentResponseUpdate(AgentTask task)
    {
        return new AgentRunResponseUpdate
        {
            AgentId = agent.Id,
            ResponseId = task.Id,
            RawRepresentation = task,
            Role = ChatRole.Assistant,
            Contents = task.ToAIContents(),
            AdditionalProperties = task.Metadata?.ToAdditionalProperties(),
        };
    }

    private AgentRunResponseUpdate ConvertToAgentResponseUpdate(TaskUpdateEvent taskUpdateEvent)
    {
        AgentRunResponseUpdate responseUpdate = new()
        {
            AgentId = agent.Id,
            ResponseId = taskUpdateEvent.TaskId,
            RawRepresentation = taskUpdateEvent,
            Role = ChatRole.Assistant,
            AdditionalProperties = taskUpdateEvent.Metadata?.ToAdditionalProperties() ?? [],
        };

        if (taskUpdateEvent is TaskArtifactUpdateEvent artifactUpdateEvent)
        {
            responseUpdate.Contents = artifactUpdateEvent.Artifact.ToAIContents();
            responseUpdate.RawRepresentation = artifactUpdateEvent;
        }

        return responseUpdate;
    }

    private static AgentMessage CreateA2AMessage(A2AAgentThread typedThread, IEnumerable<ChatMessage> messages)
    {
        var a2aMessage = messages.ToA2AMessage();

        // Linking the message to the existing conversation, if any.
        // See: https://github.com/a2aproject/A2A/blob/main/docs/topics/life-of-a-task.md#group-related-interactions
        a2aMessage.ContextId = typedThread.ContextId;

        // Link the message as a follow-up to an existing task, if any.
        // See: https://github.com/a2aproject/A2A/blob/main/docs/topics/life-of-a-task.md#task-refinements
        a2aMessage.ReferenceTaskIds = typedThread.TaskId is null ? null : [typedThread.TaskId];

        return a2aMessage;
    }
}

internal static class A2AAgentTaskExtensions
{
    internal static IList<ChatMessage>? ToChatMessages(this AgentTask agentTask)
    {
    
        List<ChatMessage>? messages = null;

        if (agentTask?.Artifacts is { Count: > 0 })
        {
            foreach (var artifact in agentTask.Artifacts)
            {
                (messages ??= []).Add(artifact.ToChatMessage());
            }
        }

        return messages;
    }

    internal static IList<AIContent>? ToAIContents(this AgentTask agentTask)
    {

        List<AIContent>? aiContents = null;

        if (agentTask.Artifacts is not null)
        {
            foreach (var artifact in agentTask.Artifacts)
            {
                (aiContents ??= []).AddRange(artifact.ToAIContents());
            }
        }

        return aiContents;
    }
}

internal static class A2AArtifactExtensions
{
    internal static ChatMessage ToChatMessage(this Artifact artifact)
    {
        return new ChatMessage(ChatRole.Assistant, artifact.ToAIContents())
        {
            AdditionalProperties = artifact.Metadata.ToAdditionalProperties(),
            RawRepresentation = artifact,
        };
    }

    internal static List<AIContent> ToAIContents(this Artifact artifact)
    {
        return artifact.Parts.ConvertAll(part => part.ToAIContent());
    }
}


internal static class A2AMetadataExtensions
{
    /// <summary>
    /// Converts a dictionary of metadata to an <see cref="AdditionalPropertiesDictionary"/>.
    /// </summary>
    /// <remarks>
    /// This method can be replaced by the one from A2A SDK once it is public.
    /// </remarks>
    /// <param name="metadata">The metadata dictionary to convert.</param>
    /// <returns>The converted <see cref="AdditionalPropertiesDictionary"/>, or null if the input is null or empty.</returns>
    internal static AdditionalPropertiesDictionary? ToAdditionalProperties(this Dictionary<string, JsonElement>? metadata)
    {
        if (metadata is not { Count: > 0 })
        {
            return null;
        }

        var additionalProperties = new AdditionalPropertiesDictionary();
        foreach (var kvp in metadata)
        {
            additionalProperties[kvp.Key] = kvp.Value;
        }
        return additionalProperties;
    }
}


internal static class ChatMessageExtensions
{
    internal static AgentMessage ToA2AMessage(this IEnumerable<ChatMessage> messages)
    {
        List<Part> allParts = [];

        foreach (var message in messages)
        {
            if (message.Contents.ToParts() is { Count: > 0 } ps)
            {
                allParts.AddRange(ps);
            }
        }

        return new AgentMessage
        {
            MessageId = Guid.NewGuid().ToString("N"),
            Role = MessageRole.User,
            Parts = allParts,
        };
    }
}

internal static class A2AAIContentExtensions
{
    /// <summary>
    ///  Converts a collection of <see cref="AIContent"/> to a list of <see cref="Part"/> objects.
    /// </summary>
    /// <param name="contents">The collection of AI contents to convert.</param>"
    /// <returns>The list of A2A <see cref="Part"/> objects.</returns>
    internal static List<Part>? ToParts(this IEnumerable<AIContent> contents)
    {
        List<Part>? parts = null;

        foreach (var content in contents)
        {
            var part = content.ToPart();
            if (part is not null)
            {
                (parts ??= []).Add(part);
            }
        }

        return parts;
    }
}