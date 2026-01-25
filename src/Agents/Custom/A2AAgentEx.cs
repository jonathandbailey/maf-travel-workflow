using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Agents.Extensions;

namespace Agents.Custom;

public class A2AAgentEx(AIAgent agent)
{
    public async IAsyncEnumerable<AgentRunResponseUpdate> RunCoreStreamingAsync(
        A2AClient client,
        IEnumerable<ChatMessage> messages, 
        A2AAgentThread thread, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        A2AAgentThread typedThread = thread;


        MessageSendParams sendParams = new()
        {
            Message = messages.CreateA2AMessage(typedThread),
        };

        var a2aSseEvents = client.SendMessageStreamingAsync(sendParams, cancellationToken).ConfigureAwait(false);

     
        await foreach (var sseEvent in a2aSseEvents)
        {
            if (sseEvent.Data is AgentMessage message)
            {
                yield return message.ConvertToAgentResponseUpdate(agent.Id);
            }
            else if (sseEvent.Data is AgentTask task)
            {
                yield return task.ConvertToAgentResponseUpdate(agent.Id);
            }
            else if (sseEvent.Data is TaskUpdateEvent taskUpdateEvent)
            {
       
                yield return taskUpdateEvent.ConvertToAgentResponseUpdate(agent.Id);
            }
            else
            {
                throw new NotSupportedException($"Only message, task, task update events are supported from A2A agents. Received: {sseEvent.Data.GetType().FullName ?? "null"}");
            }
        }
    }
}