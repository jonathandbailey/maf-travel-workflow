using Application.Agents;
using Application.Workflows.Conversations;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Tests.Common;
using Xunit.Abstractions;

namespace Tests;

public class ConversationWorkflowTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Execute_WhenActAgentRequestsUserInput_ShouldWait()
    {
        var reasonAgent = new Mock<IAgent>();

        var actAgent = new Mock<IAgent>();

        reasonAgent.SetupAgentResponse(Data.ReasonTripToParisDeparturePointRequired);
        actAgent.SetupAgentResponse(Data.ActAgentDepartureCityResponse);

        var workFlow = new ConversationWorkflow(
            reasonAgent.Object,
            actAgent.Object,
            CheckpointManager.CreateJson(new FakeCheckpointStore(outputHelper)));

        var response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.PlanTripToParisUserRequest));

        Assert.NotNull(response);
        Assert.Equal(WorkflowResponseState.UserInputRequired, response.State);
        Assert.Equal(Data.ActAgentDepartureCityUserResponse, response.Message);

        reasonAgent.SetupAgentResponse(Data.ReasonTripInformationCompete);
        actAgent.SetupAgentResponse(Data.ActAgentUserComplete);

        await workFlow.Execute(new ChatMessage(ChatRole.User, Data.UserDepartingFromCity));
    }
}