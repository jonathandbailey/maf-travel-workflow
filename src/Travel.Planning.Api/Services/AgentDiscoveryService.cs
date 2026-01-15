using A2A;

namespace Travel.Planning.Api.Services;

public class AgentDiscoveryService : IAgentDiscoveryService
{
    private readonly List<AgentCard> _agentCards =
    [
        new AgentCard
        {
            Name = "Travel_Planning",
            Description = "An agent that plans your travel.",
            Url = "https://localhost:7027/api/a2a/travel",
            Skills =
            [
                new AgentSkill
                {
                    Name = "Travel_Planning", 
                    Description = "Plan Trips",
                    InputModes = ["{ 'userMessage':'I want to plan a trip to Paris?', 'intent':'plan_travel' }"]
                }
            ],
            Version = "1.0"
        }
    ];

    public Task<AgentCard> GetAgentCard(string url)
    {
        var card = _agentCards.FirstOrDefault(ac => ac.Url == url);

        return Task.FromResult(card ?? throw new A2AException($"Card Not found with Url : {url} "));
    }
}

public interface IAgentDiscoveryService
{
    Task<AgentCard> GetAgentCard(string url);
}