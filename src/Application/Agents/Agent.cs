using Application.Infrastructure;
using Application.Observability;
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
        using var activity = Telemetry.Start("Agent");

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


        activity?.SetTag("llm.input_tokens", response.Usage?.InputTokenCount ?? 0);
        activity?.SetTag("llm.output_tokens", response.Usage?.OutputTokenCount ?? 0);
        activity?.SetTag("llm.total_tokens", response.Usage?.TotalTokenCount ?? 0);

        var threadState = thread.Serialize();

        await repository.SaveAsync(userId, sessionId, new AgentState(threadState), type.ToString());

        return response;
    }
}
public interface IAgent
{
    Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
}