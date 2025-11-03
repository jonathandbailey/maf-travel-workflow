using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.AI;
using System.Text;

namespace ConsoleApp.Workflows.Conversations.ReAct;

public class ActNode(AIAgent agent) : ReflectingExecutor<ActNode>("ActNode"), IMessageHandler<string, string>, IMessageHandler<UserResponse>

{
    private List<ChatMessage> _messages = [];

    public async ValueTask<string> HandleAsync(string message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var stringBuilder = new StringBuilder();

        var requestMessage = new ChatMessage(ChatRole.User, message);

        _messages.Add(requestMessage);

        await foreach (var update in agent.RunStreamingAsync(requestMessage, cancellationToken: cancellationToken))
        {
            stringBuilder.Append(update.Text);
            await context.AddEventAsync(new ConversationStreamingEvent(update.Text), cancellationToken);
        }
    
        var response = stringBuilder.ToString();

        _messages.Add(new ChatMessage(ChatRole.Assistant, response));

        await context.SendMessageAsync(new UserRequest() {Message = response}, cancellationToken: cancellationToken);

        return  stringBuilder.ToString();
    }

    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return context.QueueStateUpdateAsync("ChatHistory-Act", _messages, cancellationToken);
    }

    protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _messages = await context.ReadStateAsync<List<ChatMessage>>("ChatHistory-Act");
    }

    public async ValueTask HandleAsync(UserResponse message, IWorkflowContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}

public class ConversationStreamingEvent(string message) : WorkflowEvent(message) { }

public class WaitForUserInputEvent() : WorkflowEvent() { }