namespace Agents.Dto;

class SnapShot<T>(string type, T payload)
{
    public string Type { get; } = type;
    public T Payload { get; } = payload;
}

public class StatusUpdate(string type, string source, string status, string details)
{
    public string Type { get; } = type;

    public string Source { get; } = source;

    public string Status { get; } = status;

    public string Details { get; } = details;
}

public class ArtifactCreated(string type, Guid id, string key)
{
    public Guid Id { get; } = id;
    public string Key { get; } = key;
    public string Type { get; } = type;
}