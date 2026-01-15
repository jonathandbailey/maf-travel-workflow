using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Workflows.Dto;
using Workflows.Events;

namespace Workflows;

public class TravelWorkflow(
    Workflow workflow,
    CheckpointManager checkpointManager,
    CheckpointInfo? checkpointInfo,
    WorkflowState state,
    ILogger logger)
{
    private CheckpointManager CheckpointManager { get; set; } = checkpointManager;

    public CheckpointInfo? CheckpointInfo { get; set; } = checkpointInfo;

    public WorkflowState State { get; set; } = state;

    public async Task<WorkflowResponse> Execute(TravelWorkflowRequestDto requestDto)
    {
     
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

            if (evt is TravelWorkflowErrorEvent travelWorkflowErrorEvent)
            {
                logger.LogError(travelWorkflowErrorEvent.Exception, $"Travel Workflow Error:{travelWorkflowErrorEvent.Message}, {travelWorkflowErrorEvent.Description}");
                return new WorkflowResponse(WorkflowState.Error, "Travel Request has failed.");
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



