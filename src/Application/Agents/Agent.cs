using Application.Infrastructure;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Application.Agents;

public class Agent(AIAgent agent, IAgentThreadRepository repository, AgentTypes type) : IAgent
{
    public async Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        AgentThread? thread;
        
        if (!await repository.ExistsAsync(userId, sessionId, type.ToString()))
        {
            thread = agent.GetNewThread();
        }
        else
        { 
           var stateDto = await repository.LoadAsync(userId, sessionId, type.ToString());

           thread = agent.DeserializeThread(stateDto.Thread);
        }

        var response =  await agent.RunAsync(messages, thread, cancellationToken: cancellationToken);

        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());

        return response;
    }
}
public interface IAgent
{
    Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
}