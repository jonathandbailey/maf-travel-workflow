using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text;

namespace ConsoleApp.Workflows.Conversations.ReAct;

public class ActNode(AIAgent agent) : ReflectingExecutor<ActNode>("ActNode"), IMessageHandler<string, string>
{
    public async ValueTask<string> HandleAsync(string message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var stringBuilder = new StringBuilder();

        await foreach (var update in agent.RunStreamingAsync(new ChatMessage(ChatRole.User, message), cancellationToken: cancellationToken))
        {
            stringBuilder.Append(update.Text);
            await context.AddEventAsync(new ConversationStreamingEvent(update.Text), cancellationToken);
        }
    
        var response = stringBuilder.ToString();

        await context.SendMessageAsync(new UserRequest() {Message = response}, cancellationToken: cancellationToken);

        return  stringBuilder.ToString();
    }
}

public class ConversationStreamingEvent(string message) : WorkflowEvent(message) { }

public class WaitForUserInputEvent() : WorkflowEvent() { }