using Agents;
using Agents.Services;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace Api.Extensions;

public static class AgUiExtensions
{
    public static async Task<WebApplication> MapAgUiToAgent(this WebApplication app)
    {
        var discovery = app.Services.GetRequiredService<IA2AAgentServiceDiscovery>();

        await discovery.Initialize();

        var agentFactory = app.Services.GetRequiredService<IAgentFactory>();
    
        var agent = await agentFactory.CreateUserAgent(discovery.GetTools());

        app.MapAGUI("ag-ui", agent);
        
        return app;
    }
}