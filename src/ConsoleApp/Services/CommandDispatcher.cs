using ConsoleApp.Dto;
using ConsoleApp.Settings;

namespace ConsoleApp.Services;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IChatClient _httpChatClient;
    private readonly Dictionary<string, Func<string, Task>> _commands = new();
    private Guid _sessionId = Guid.NewGuid();
   

    public CommandDispatcher(IChatClient httpChatClient)
    {
        _httpChatClient = httpChatClient;
    }

    public void Initialize(CancellationTokenSource cancellationTokenSource)
    {
        _commands[Constants.ExitCommandKey] = async _ => await cancellationTokenSource.CancelAsync();
        _commands[Constants.ChatCommandKey] = async input => await ChatViaApi(input);
    }
    
    public async Task ExecuteCommandAsync(string input)
    {
        if (_commands.TryGetValue(input, out var command))
        {
            await command(input);
            return;
        }

        await _commands[Constants.ChatCommandKey](input);
    }
   
    private async Task ChatViaApi(string input)
    {
        var userResponse = await _httpChatClient.Send(new UserRequestDto { Message = input, SessionId = _sessionId});

        
        Console.WriteLine(userResponse.Message);
    
        _sessionId = userResponse.SessionId;
       
    }
}

public interface ICommandDispatcher
{
    void Initialize(CancellationTokenSource cancellationTokenSource);
    Task ExecuteCommandAsync(string input);
}