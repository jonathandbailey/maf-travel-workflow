using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Agents.Extensions;

public static class A2aExtensions
{
    public static AgentRunResponseUpdate ConvertToAgentResponseUpdate(this AgentMessage message, string agentId)
    {
        return new AgentRunResponseUpdate
        {
            AgentId = agentId,
            ResponseId = message.MessageId,
            RawRepresentation = message,
            Role = ChatRole.Assistant,
            MessageId = message.MessageId,
            Contents = message.Parts.ConvertAll(part => part.ToAIContent()),
            AdditionalProperties = message.Metadata?.ToAdditionalProperties(),
        };
    }

    public static AgentRunResponseUpdate ConvertToAgentResponseUpdate(this AgentTask task, string agentId)
    {
        return new AgentRunResponseUpdate
        {
            AgentId = agentId,
            ResponseId = task.Id,
            RawRepresentation = task,
            Role = ChatRole.Assistant,
            Contents = task.ToAiContents(),
            AdditionalProperties = task.Metadata?.ToAdditionalProperties(),
        };
    }

    public static AgentRunResponseUpdate ConvertToAgentResponseUpdate(this TaskUpdateEvent taskUpdateEvent, string agentId)
    {
        AgentRunResponseUpdate responseUpdate = new()
        {
            AgentId = agentId,
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

    public static AgentMessage CreateA2AMessage(this IEnumerable<ChatMessage> messages, A2AAgentThread typedThread )
    {
        var a2aMessage = messages.ToA2AMessage();

        a2aMessage.ContextId = typedThread.ContextId;

        a2aMessage.ReferenceTaskIds = typedThread.TaskId is null ? null : [typedThread.TaskId];

        return a2aMessage;
    }


    public static AgentMessage ToA2AMessage(this IEnumerable<ChatMessage> messages)
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

    public static IList<ChatMessage>? ToChatMessages(this AgentTask agentTask)
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

    public static IList<AIContent>? ToAiContents(this AgentTask agentTask)
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

    public static List<Part>? ToParts(this IEnumerable<AIContent> contents)
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

    public static AdditionalPropertiesDictionary? ToAdditionalProperties(this Dictionary<string, JsonElement>? metadata)
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

    public static ChatMessage ToChatMessage(this Artifact artifact)
    {
        return new ChatMessage(ChatRole.Assistant, artifact.ToAIContents())
        {
            AdditionalProperties = artifact.Metadata.ToAdditionalProperties(),
            RawRepresentation = artifact,
        };
    }



    public static List<AIContent> ToAIContents(this Artifact artifact)
    {
        return artifact.Parts.ConvertAll(part => part.ToAIContent());
    }
}