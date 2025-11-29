using Application.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;

namespace Tests.Common;

public static class TestHelperExtensions
{
    public static void SetupAgentResponse(this Mock<IAgent> agent, string response)
    {
        var agentRunResponse = new AgentRunResponse(new ChatMessage(ChatRole.Assistant, response));

        agent.Setup(x => x.RunAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(agentRunResponse);
    }
}