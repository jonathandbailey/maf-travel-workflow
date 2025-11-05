using Application.Agents;
using Application.Workflows.Conversations.Dto;
using Application.Workflows.Conversations.Nodes;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations;

public class ConversationWorkflow(IAgent reasonAgent, IAgent actAgent, CheckpointManager checkpointManager)
{
    public async Task<WorkflowResponse> Execute(ChatMessage message)
    {
        var inputPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);

        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, inputPort);
        builder.AddEdge(inputPort, actNode);

        var workflow = await builder.BuildAsync<ChatMessage>();

        var run = await InProcessExecution.StreamAsync(workflow, message, checkpointManager);

        await foreach (var evt in run.Run.WatchStreamAsync())
        {
            if (evt is RequestInfoEvent requestInfoEvent)
            {
                return Handle(requestInfoEvent);
            }
        }

        return new WorkflowResponse(WorkflowResponseState.Completed, string.Empty);
    }

    private static WorkflowResponse Handle(RequestInfoEvent requestInfoEvent)
    {
        var data = requestInfoEvent.Data as ExternalRequest;

        if (data?.Data == null)
        {
            return new WorkflowResponse(WorkflowResponseState.Error,
                "Invalid request event: missing data");
        }

        if (data.Data.AsType(typeof(UserRequest)) is not UserRequest userRequest)
        {
            return new WorkflowResponse(WorkflowResponseState.Error,
                "Invalid request event: unable to parse UserRequest");
        }

        if (string.IsNullOrWhiteSpace(userRequest.Message))
        {
            return new WorkflowResponse(WorkflowResponseState.Error,
                "Invalid request event: UserRequest message is empty");
        }

        return new WorkflowResponse(WorkflowResponseState.UserInputRequired, userRequest.Message);
    }
}