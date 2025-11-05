using Application.Agents;
using Application.Workflows.Conversations;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Moq;
using Xunit.Abstractions;


namespace Tests;

public class ConversationWorkflowTests
{
    private readonly ITestOutputHelper _outputHelper;

    public ConversationWorkflowTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task Execute_WhenActAgentRequestsUserInput_ShouldReturnUserInputRequiredState()
    {
        var reasonAgent = new Mock<IAgent>();

        var actAgent = new Mock<IAgent>();
   
        reasonAgent.SetupAgentResponse(Data.ReasonTripToParisDeparturePointRequired);
        actAgent.SetupAgentResponse(Data.ActAgentDepartureCityResponse);
  
        var workFlow = new ConversationWorkflow(reasonAgent.Object, actAgent.Object, CheckpointManager.CreateJson(new FakeCheckpointStore(_outputHelper)));

        var response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.PlanTripToParisUserRequest));

        Assert.NotNull(response);
        Assert.Equal(WorkflowResponseState.UserInputRequired, response.State);
        Assert.Equal(Data.ActAgentDepartureCityUserResponse, response.Message);
    }
}