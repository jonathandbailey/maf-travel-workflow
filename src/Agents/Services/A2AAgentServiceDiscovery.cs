using System.ComponentModel;
using A2A;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Extensions.AI;

namespace Agents.Services;

public class A2AAgentServiceDiscovery : IA2AAgentServiceDiscovery
{
    private const string _endppoint = "https://localhost:7027";

    private readonly List<AgentToolSettings> _agentToolSettings =
    [
        new AgentToolSettings()
        {
            CardPath = "/api/a2a/travel/v1/card"
        }
    ];

    private readonly List<AgentMeta> _agentMetas = [];

    public List<AITool> GetTools()
    {
        return _agentMetas.Select(x => x.Tool).ToList();
    }

    public AgentMeta GetAgentMeta(string name)
    {
        var agentMeta = _agentMetas.FirstOrDefault(x => x.Name == name);

        if (agentMeta == null)
        {
            throw new ArgumentException($"Agent meta not found for name: {name}");
        }

        return agentMeta;
    }

    public async Task Initialize()
    {
        foreach (var agentToolSetting in _agentToolSettings)
        {
            var cardResolver = new A2ACardResolver(new Uri(_endppoint), new HttpClient(), agentCardPath: agentToolSetting.CardPath);

            var card = await cardResolver.GetAgentCardAsync();

            var client = new A2AClient(new Uri(card.Url), new HttpClient());

            var agent = new A2AAgent(client, name: card.Name, description: card.Description);

            var tool = CreateFunctionTool(agent, card).AsDeclarationOnly();

            var agentMeta = new AgentMeta(card.Name, agent, card, tool, client);

            _agentMetas.Add(agentMeta);
        }
    }

    static AIFunction CreateFunctionTool(AIAgent a2aAgent, AgentCard agentCard)
    {
        var skill = agentCard.Skills.First();
        
            AIFunctionFactoryOptions options = new()
            {
                Name =skill.Name,
                Description = $$"""
                                {
                                    "description": "{{skill.Description}}",
                                    "tags": "[{{string.Join(", ", skill.Tags ?? [])}}]",
                                    "examples": "[{{string.Join(", ", skill.Examples ?? [])}}]",
                                    "inputModes": "[{{string.Join(", ", skill.InputModes ?? [])}}]",
                                    "outputModes": "[{{string.Join(", ", skill.OutputModes ?? [])}}]"
                                }
                                """,
            };

          return AIFunctionFactory.Create(RunAgentAsync, options);
       
        async Task<string> RunAgentAsync(
            [Description("The JSON payload to send to the agent")]
            string jsonPayload, CancellationToken cancellationToken)
        {
            var response = await a2aAgent.RunAsync(jsonPayload, cancellationToken: cancellationToken);

            return response.Text;
        }
    }
}

public class AgentToolSettings
{
    public required string CardPath { get; init; }
}

public class AgentMeta(string name, A2AAgent agent, AgentCard card, AITool tool, A2AClient client)
{
    public string Name { get; } = name;

    public A2AAgent Agent { get; } = agent;

    public AgentCard Card { get; } = card;

    public AITool Tool { get; } = tool;
    public A2AClient Client { get; } = client;
}

public interface IA2AAgentServiceDiscovery
{
    Task Initialize();
    List<AITool> GetTools();
    AgentMeta GetAgentMeta(string name);
}