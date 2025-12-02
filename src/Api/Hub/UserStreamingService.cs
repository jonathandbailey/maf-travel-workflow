using Api.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Text;

namespace Api.Hub;

public class UserStreamingService(IHubContext<UserHub> hub, IUserConnectionManager userConnectionManager, IOptions<HubSettings> hubSettings) : IUserStreamingService
{
    private readonly StringBuilder _buffer = new();
    private bool _canStream = true;
    private const string JsonToken = "<JSON>";

    public async Task Stream(Guid userId, string content)
    {
        _buffer.Append(content);

        if (!_canStream) return;

        var bufferStr = _buffer.ToString();

        if (string.IsNullOrEmpty(bufferStr)) return;

        if (bufferStr.Contains(JsonToken, StringComparison.Ordinal))
        {
            bufferStr = bufferStr.Replace(JsonToken, string.Empty, StringComparison.Ordinal);
  
            _buffer.Clear();
            _buffer.Append(bufferStr);

            _canStream = false;
        }

        var partialFound = false;
        var maxCheck = Math.Min(bufferStr.Length, JsonToken.Length - 1);
        
        for (var k = maxCheck; k > 0; k--)
        {
            if (bufferStr.EndsWith(JsonToken.Substring(0, k), StringComparison.Ordinal))
            {
                partialFound = true;
                break;
            }
        }

        if (partialFound) return;

        if (string.IsNullOrWhiteSpace(bufferStr))
        {
            _buffer.Clear();
            return;
        }

        var connections = userConnectionManager.GetConnections(userId);

        foreach (var connectionId in connections)
        {
            await hub.Clients.Client(connectionId).SendAsync(hubSettings.Value.PromptChannel, new UserResponseDto() { Message = bufferStr});
        }

        _buffer.Clear();
    }
}

