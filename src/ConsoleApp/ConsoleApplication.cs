using ConsoleApp.Services;
using ConsoleApp.Settings;

namespace ConsoleApp;

public class ConsoleApplication(ICommandDispatcher commandManager, IChatClient chatClient) 
{
    public async Task RunAsync(CancellationTokenSource cancellationTokenSource)
    {
        commandManager.Initialize(cancellationTokenSource);

        await chatClient.InitializeStreamingConnectionAsync();

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            Console.Write(Constants.UserCaret);

            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input)) continue;

            await commandManager.ExecuteCommandAsync(input);
        }
    }
}