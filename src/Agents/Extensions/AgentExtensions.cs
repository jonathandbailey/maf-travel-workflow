using A2A;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Agents.Dto;

namespace Agents.Extensions;

public static class AgentExtensions
{
    private const string ApplicationJsonMediaType = "application/json";

    public static string GetPartText(this TaskStatusUpdateEvent taskStatusUpdateEvent)
    {
        if (taskStatusUpdateEvent.Status.Message == null)
        {
            throw new ArgumentException("The provided TaskStatusUpdateEvent must have a Status.Message set.");
        }

        var part = taskStatusUpdateEvent.Status.Message.Parts.OfType<TextPart>().FirstOrDefault();

        return part == null ? throw new ArgumentException("The provided TaskStatusUpdateEvent must have a TextPart.") : part.Text;
    }


    public static StatusUpdate GetPartStatusDataText(this TaskStatusUpdateEvent taskStatusUpdateEvent)
    {
        if (taskStatusUpdateEvent.Status.Message == null)
        {
            throw new ArgumentException("The provided TaskStatusUpdateEvent must have a Status.Message set.");
        }

        var dataPart = taskStatusUpdateEvent.Status.Message.Parts.OfType<DataPart>().FirstOrDefault();

        if (dataPart == null)
        {
            throw new ArgumentException("The provided TaskStatusUpdateEvent must have a DataPart.");
        }

        if (!dataPart.Data.ContainsKey("status"))
        {
            throw new ArgumentException("The provided TaskStatusUpdateEvent DataPart must contain a 'status' key.");
        }

        var statusPayload = dataPart.Data["status"];

        var statusUpdate = JsonSerializer.Deserialize<StatusUpdate>(statusPayload);

        return statusUpdate ?? throw new ArgumentException("The provided TaskStatusUpdateEvent DataPart 'status' key must be a valid StatusUpdate.");
    }

    public static ArtifactCreated GetPartArtifactDataText(this TaskArtifactUpdateEvent taskStatusUpdateEvent)
    {
        var dataPart = taskStatusUpdateEvent.Artifact.Parts.OfType<DataPart>().FirstOrDefault();

        if (dataPart == null)
        {
            throw new ArgumentException("The provided TaskArtifactUpdateEvent must have a DataPart.");
        }

        if (!dataPart.Data.ContainsKey("artifact"))
        {
            throw new ArgumentException("The provided TaskArtifactUpdateEvent DataPart must contain an 'artifact' key.");
        }

        var statusPayload = dataPart.Data["artifact"];

        var artifactCreated = JsonSerializer.Deserialize<ArtifactCreated>(statusPayload);

        return artifactCreated ?? throw new ArgumentException("The provided TaskArtifactUpdateEvent DataPart 'artifact' key must be a valid ArtifactCreated.");
    }

    public static void AddToolCalls(this Dictionary<string, FunctionCallContent> tools, IList<AIContent> contents)
    {
        foreach (var content in contents)
        {
            if (content is FunctionCallContent callContent)
            {
                tools[callContent.Name] = callContent;
            }
        }
    }

    public static AgentRunResponseUpdate ToAgentResponseStatusMessage(this StatusUpdate statusUpdate)
    {
        var snapshot = new SnapShot<StatusUpdate>(statusUpdate.Type, statusUpdate);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);

        return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }

    public static AgentRunResponseUpdate ToAgentResponseStatusMessage(this ArtifactCreated artifactCreated)
    {
        var snapshot = new SnapShot<ArtifactCreated>(artifactCreated.Type, artifactCreated);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);

        return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }

    public static AgentRunResponseUpdate ToAgentResponseStatusMessage(this string message)
    {
        var statusUpdate = new StatusUpdate("StatusUpdate", "Conversation Agent", message, string.Empty);

        var snapshot = new SnapShot<StatusUpdate>(statusUpdate.Type, statusUpdate);

        var stateBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);

        return new AgentRunResponseUpdate
        {
            Contents = [new DataContent(stateBytes, ApplicationJsonMediaType)]
        };
    }
}