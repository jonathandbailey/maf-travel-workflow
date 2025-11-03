using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;

namespace ConsoleApp.Workflows.Conversations.ReAct;

public class ReasonNode(AIAgent agent) : ReflectingExecutor<ReasonNode>("ReasonNode"), IMessageHandler<string, string>
{
    private string _message = string.Empty;
    
    public async ValueTask<string> HandleAsync(string message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var response = await agent.RunAsync(new ChatMessage(ChatRole.User, message), cancellationToken: cancellationToken);

        _message = message;

        return response.Text;
    }

    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return context.QueueStateUpdateAsync("ReasonNodeKey", _message, cancellationToken);
    }
}