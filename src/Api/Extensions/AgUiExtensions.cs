using Application.Agents;
using Application.Services;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace Api.Extensions;

public static class AgUiExtensions
{
    public static async Task<WebApplication> MapAgUiToAgent(this WebApplication app)
    {
        var agentFactory = app.Services.GetRequiredService<IAgentFactory>();

        var travelWorkflowService = app.Services.GetRequiredService<ITravelWorkflowService>();

        var agent = await agentFactory.CreateConversationAgent(travelWorkflowService);

        app.MapAGUI("ag-ui", agent);
        
        return app;
    }
}