using Application.Agents;
using Application.Observability;
using Application.Workflows.Conversations.Dto;
using Application.Workflows.Conversations.Nodes;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Application.Workflows.Conversations;

public class ConversationWorkflow(IAgent reasonAgent, IAgent actAgent, IWorkflowManager workflowManager)
{
    private WorkflowState _state  = WorkflowState.Initialized;
    private CheckpointInfo? _checkpointInfo = null;

    public async Task<WorkflowResponse> Execute(ChatMessage message)
    {
        var inputPort = RequestPort.Create<UserRequest, UserResponse>("user-input");

        var reasonNode = new ReasonNode(reasonAgent);
        var actNode = new ActNode(actAgent);

        var builder = new WorkflowBuilder(reasonNode);

        builder.AddEdge(reasonNode, actNode);
        builder.AddEdge(actNode, inputPort);
        builder.AddEdge(inputPort, actNode);
        builder.AddEdge(actNode, reasonNode);

        var workflow = await builder.BuildAsync<ChatMessage>();

        var run = await CreateStreamingRun(workflow, message);

        await foreach (var evt in run.Run.WatchStreamAsync())
        {
            _state = _state == WorkflowState.Initialized ? WorkflowState.Executing : _state;
            
            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

                if (checkpoint != null)
                {
                    _checkpointInfo = checkpoint;
                }
            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                switch (_state)
                {
                    case WorkflowState.Executing:
                    {
                        return HandleRequestForUserInput(requestInfoEvent);
                    }
                    case WorkflowState.WaitingForUserInput:
                    {
                        var resp = requestInfoEvent.Request.CreateResponse(new UserResponse(message.Text));

                        _state = WorkflowState.Executing;
                        await run.Run.SendResponseAsync(resp);
                        break;
                    }
                }
            }
        }

        return new WorkflowResponse(WorkflowResponseState.Completed, string.Empty);
    }

    private async Task<Checkpointed<StreamingRun>> CreateStreamingRun(Workflow<ChatMessage> workflow,
        ChatMessage message)
    {
        switch (_state)
        {
            case WorkflowState.Initialized:
                return await InProcessExecution.StreamAsync(workflow, message, workflowManager.CheckpointManager);
            case WorkflowState.WaitingForUserInput:
                var activity = Telemetry.StarActivity("Workflow-[resume]");
                activity?.SetTag("RunId", _checkpointInfo.RunId);
                activity?.SetTag("CheckpointId", _checkpointInfo.CheckpointId);
                var run =  await InProcessExecution.ResumeStreamAsync(workflow, _checkpointInfo, workflowManager.CheckpointManager,
                    _checkpointInfo.RunId);
                activity?.Dispose();
                return run;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private WorkflowResponse HandleRequestForUserInput(RequestInfoEvent requestInfoEvent)
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

        _state = WorkflowState.WaitingForUserInput;

        return new WorkflowResponse(WorkflowResponseState.UserInputRequired, userRequest.Message);
    }
}

public enum WorkflowState
{
    Initialized,
    Executing,
    WaitingForUserInput,
    Completed,
    Error
}