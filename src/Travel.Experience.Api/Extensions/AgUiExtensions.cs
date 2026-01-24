using Agents.Services;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Travel.Experience.Api.Agents;

namespace Travel.Experience.Api.Extensions;

public static class AgUiExtensions
{
    public static async Task<WebApplication> MapAgUiToAgent(this WebApplication app)
    {
        var discovery = app.Services.GetRequiredService<IA2AAgentServiceDiscovery>();

        await discovery.Initialize();

        var agentFactory = app.Services.GetRequiredService<IAgentFactory>();

        var agent = await agentFactory.Create("conversation_agent", tools: discovery.GetTools());

        var conversationAgent = new ConversationAgent(agent, discovery);

      
        var agui =  agentFactory.UseMiddleware(conversationAgent, "agent-thread");
        var thread = agentFactory.UseMiddleware(agui, "agent-ag-ui");

        app.MapAGUI("ag-ui", thread);

        return app;
    }
}