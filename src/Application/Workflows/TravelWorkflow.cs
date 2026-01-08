using Application.Interfaces;
using Application.Observability;
using Application.Workflows.Dto;
using Application.Workflows.Events;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Application.Dto;

namespace Application.Workflows;

public class TravelWorkflow(
    Workflow workflow,
    CheckpointManager checkpointManager,
    CheckpointInfo? checkpointInfo,
    WorkflowState state,
    IUserStreamingService userStreamingService,
    ILogger logger)
{
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

        var run = await workflow.CreateStreamingRun(
            new ReasoningInputDto(requestDto.Message.Text), State, CheckpointManager, CheckpointInfo);

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
                await userStreamingService.Artifact(artifactStatusEvent.Key);
            }

            if (evt is ReasonActWorkflowCompleteEvent reasonActWorkflowCompleteEvent)
            {
                await userStreamingService.Stream(string.Empty, true);

                return new WorkflowResponse(WorkflowState.Completed, reasonActWorkflowCompleteEvent.Message);
            }

            if (evt is TravelWorkflowErrorEvent travelWorkflowErrorEvent)
            {
                logger.LogError(travelWorkflowErrorEvent.Exception, $"Travel Workflow Error:{travelWorkflowErrorEvent.Message}, {travelWorkflowErrorEvent.Description}");
                return new WorkflowResponse(WorkflowState.Error, "Travel Request has failed.");
            }

            if (evt is TravelPlanUpdatedEvent)
            {
                await userStreamingService.TravelPlan();
            }

            if (evt is WorkflowStatusEvent statusEvent)
            {
                await userStreamingService.Status(statusEvent.Status, statusEvent.Details, statusEvent.Source);
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
                        var resp = requestInfoEvent.Request.CreateResponse(new ReasoningInputDto(requestDto.Message.Text));

                        State = WorkflowState.Executing;
                        await run.Run.SendResponseAsync(resp);
                        break;
                    }
                }
            }
        }

        

        State = WorkflowState.Completed;

        return new WorkflowResponse(State, string.Empty);
    }
}



