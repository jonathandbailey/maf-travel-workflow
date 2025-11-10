using Application.Agents;
using Application.Infrastructure;
using Application.Workflows;
using Application.Workflows.Conversations;
using Application.Workflows.Conversations.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Moq;
using Tests.Common;
using Xunit.Abstractions;

namespace Tests;

public class ReActWorkflowTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Execute_WhenActAgentRequestsUserInput_ShouldWait()
    {
        var sessionId = Guid.NewGuid();
        
        var reasonAgent = new Mock<IAgent>();

        var actAgent = new Mock<IAgent>();

        var repositoryMock = new Mock<IAzureStorageRepository>();
        var checkpointRepositoryMock = new Mock<ICheckpointRepository>();

        repositoryMock.Setup(x => x.BlobExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var settingsMock = new Mock<IOptions<AzureStorageSeedSettings>>();

        settingsMock.Setup(x => x.Value).Returns(new AzureStorageSeedSettings { ContainerName = "workflow"});

        reasonAgent.SetupAgentResponse(Data.ReasonTripToParisDeparturePointRequired);
        actAgent.SetupAgentResponse(Data.ActAgentDepartureCityResponse);

        var workflowManager = new WorkflowRepository(repositoryMock.Object, settingsMock.Object);

        await workflowManager.LoadAsync(sessionId);

        var workFlow = new ReActWorkflow(
            reasonAgent.Object,
            actAgent.Object, CheckpointManager.CreateJson(new FakeCheckpointStore(outputHelper)), null, WorkflowState.Initialized);

        var response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.PlanTripToParisUserRequest));

        Assert.NotNull(response);
        Assert.Equal(WorkflowState.WaitingForUserInput, response.State);
        Assert.Equal(Data.ActAgentDepartureCityUserResponse, response.Message);

        reasonAgent.SetupAgentResponse(Data.ReasonTripInformationCompete);
        actAgent.SetupAgentResponse(Data.ActAgentUserComplete);

        response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.UserDepartingFromCity));
    }
}