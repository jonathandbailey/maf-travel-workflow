using Api.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text;

namespace Api.Hub;

public class UserStreamingService(IHubContext<UserHub> hub, IUserConnectionManager userConnectionManager, IOptions<HubSettings> hubSettings) : IUserStreamingService
{
    public async Task Stream(Guid userId, string content)
    {
        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync(hubSettings.Value.PromptChannel, new UserResponseDto() { Message = content });
        }
    }

    public async Task Status(Guid userId, string content)
    {
        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync("status", new UserResponseDto() { Message = content });
        }
    }
}

