using Api.Settings;
using Application.Interfaces;
using Application.Users;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Api.Hub;

public class UserStreamingService(
    IHubContext<UserHub> hub, 
    IUserConnectionManager userConnectionManager,
    IExecutionContextAccessor sessionContextAccessor,
    IOptions<HubSettings> hubSettings) : IUserStreamingService
{
    public async Task Stream(string content, bool isEndOfStream)
    {
        var connections = userConnectionManager.GetConnections(sessionContextAccessor.Context.UserId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync(hubSettings.Value.PromptChannel, new UserResponseDto() { Message = content, IsEndOfStream = isEndOfStream, Id = sessionContextAccessor.Context.RequestId});
        }
    }

    public async Task Status(string content, string details, string source)
    {
        var connections = userConnectionManager.GetConnections(sessionContextAccessor.Context.UserId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync("status", new StatusDto(content, details, sessionContextAccessor.Context.RequestId, source));
        }
    }

    public async Task TravelPlan()
    {
        var connections = userConnectionManager.GetConnections(sessionContextAccessor.Context.UserId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync("travelPlan");
        }
    }

    public async Task Artifact(string key)
    {
        var connections = userConnectionManager.GetConnections(sessionContextAccessor.Context.UserId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync("artifact", new ArtifactStatusDto(key));
        }
    }
}

