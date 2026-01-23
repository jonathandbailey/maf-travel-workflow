using Agents;
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
    
        var agent = await agentFactory.Create("conversation_agent",discovery.GetTools());

        var conversationAgent = new ConversationAgent(agent, discovery);

        var extended = agentFactory.ExtendConversationAgent(conversationAgent);

        app.MapAGUI("ag-ui", extended);

        return app;
    }
}