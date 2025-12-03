using Application.Interfaces;
using Application.Observability;
using Application.Workflows.Events;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Workflows;

public class TravelWorkflow(
    Workflow workflow,
    CheckpointManager checkpointManager,
    CheckpointInfo? checkpointInfo,
    WorkflowState state,
    IUserStreamingService userStreamingService,
    ILogger logger)
{
    private readonly List<ArtifactStatusEvent> _artifactStatusEvents = [];
    
    private CheckpointManager CheckpointManager { get; set; } = checkpointManager;

    public CheckpointInfo? CheckpointInfo { get; private set; } = checkpointInfo;

    public WorkflowState State { get; private set; } = state;

    public async Task<WorkflowResponse> Execute(TravelWorkflowRequestDto requestDto)
    {
        using var activity = Telemetry.Start("WorkflowExecute");

        activity?.AddTag("workflow.state", State.ToString());
        activity?.AddTag("workflow.instance.id", CheckpointInfo?.CheckpointId);
        activity?.AddTag("workflow.has_checkpoint", (CheckpointInfo != null).ToString());
        activity?.AddTag("workflow.user.message", requestDto.Message.Text);

        var run = await workflow.CreateStreamingRun(requestDto, State, CheckpointManager, CheckpointInfo);

        await foreach (var evt in run.Run.WatchStreamAsync())
        {
            if (State == WorkflowState.Initialized)
            {
                State = WorkflowState.Executing;
            }

            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                var checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;

                if (checkpoint != null)
                {
                    CheckpointInfo = checkpoint;
                }
            }

            if (evt is ArtifactStatusEvent artifactStatusEvent)
            {
                _artifactStatusEvents.Add(artifactStatusEvent);
            }

            if (evt is ReasonActWorkflowCompleteEvent reasonActWorkflowCompleteEvent)
            {
                return new WorkflowResponse(WorkflowState.Completed, reasonActWorkflowCompleteEvent.Message);
            }

            if (evt is TravelWorkflowErrorEvent travelWorkflowErrorEvent)
            {
                logger.LogError(travelWorkflowErrorEvent.Exception, "Travel Workflow Error");
                return new WorkflowResponse(WorkflowState.Error, "Travel Request has failed.");
            }

            if (evt is ConversationStreamingEvent streamingEvent)
            {
                await userStreamingService.Stream(requestDto.UserId, streamingEvent.Content, streamingEvent.EndOfStream, requestDto.RequestId);
            }

            if (evt is WorkflowStatusEvent statusEvent)
            {
                await userStreamingService.Status(requestDto.UserId, statusEvent.Status);
            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                switch (State)
                {
                    case WorkflowState.Executing:
                    {
                        var response = requestInfoEvent.HandleRequestForUserInput();

                        State = WorkflowState.WaitingForUserInput;

                        return response;
                    }
                    case WorkflowState.WaitingForUserInput:
                    {
                        var resp = requestInfoEvent.Request.CreateResponse(new UserResponse(requestDto.Message.Text, requestDto.RequestId));

                        State = WorkflowState.Executing;
                        await run.Run.SendResponseAsync(resp);
                        break;
                    }
                }
            }
        }

        var stringBuilder = new StringBuilder();

        foreach (var statusEvent in _artifactStatusEvents)
        {
            stringBuilder.AppendLine(statusEvent.ToString());
        }

        return new WorkflowResponse(WorkflowState.Completed, stringBuilder.ToString());
    }
}



