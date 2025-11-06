using Api.Infrastructure.Settings;
using Application.Agents;
using Application.Infrastructure;
using Application.Workflows;
using Application.Workflows.Conversations;
using Application.Workflows.Conversations.Dto;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Moq;
using Tests.Common;
using Xunit.Abstractions;

namespace Tests;

public class ConversationWorkflowTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Execute_WhenActAgentRequestsUserInput_ShouldWait()
    {
        var sessionId = Guid.NewGuid();
        
        var reasonAgent = new Mock<IAgent>();

        var actAgent = new Mock<IAgent>();

        var repositoryMock = new Mock<IAzureStorageRepository>();

        repositoryMock.Setup(x => x.BlobExists(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        var settingsMock = new Mock<IOptions<AzureStorageSeedSettings>>();

        reasonAgent.SetupAgentResponse(Data.ReasonTripToParisDeparturePointRequired);
        actAgent.SetupAgentResponse(Data.ActAgentDepartureCityResponse);

        var workflowManager = new WorkflowManager(new FakeCheckpointStore(outputHelper), repositoryMock.Object,
            settingsMock.Object);

        await workflowManager.Initialize(sessionId);

        var workFlow = new ConversationWorkflow(
            reasonAgent.Object,
            actAgent.Object, workflowManager);

        var response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.PlanTripToParisUserRequest));

        Assert.NotNull(response);
        Assert.Equal(WorkflowResponseState.UserInputRequired, response.State);
        Assert.Equal(Data.ActAgentDepartureCityUserResponse, response.Message);

        reasonAgent.SetupAgentResponse(Data.ReasonTripInformationCompete);
        actAgent.SetupAgentResponse(Data.ActAgentUserComplete);

        response = await workFlow.Execute(new ChatMessage(ChatRole.User, Data.UserDepartingFromCity));
    }
}