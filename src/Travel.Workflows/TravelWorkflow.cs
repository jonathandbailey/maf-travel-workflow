using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;

namespace Travel.Workflows;

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

    public async IAsyncEnumerable<WorkflowResponse> Execute(TravelWorkflowRequestDto requestDto)
    {

        var startWorkflow = new StartWorkflowDto(requestDto.ThreadId, new ReasoningInputDto(requestDto.Message.Text));

        var run = await workflow.CreateStreamingRun(
            startWorkflow, State, CheckpointManager, CheckpointInfo);

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

            if(evt is WorkflowStatusEvent workflowStatusEvent)
            {
                var statusUpdate = new StatusUpdate(workflowStatusEvent.Source, workflowStatusEvent.Status,
                    workflowStatusEvent.Details);

                var serialized = JsonSerializer.SerializeToElement(statusUpdate);

                yield return new WorkflowResponse(WorkflowState.Executing, workflowStatusEvent.Status, WorkflowAction.StatusUpdate, serialized);
            }

            if (evt is ArtifactStatusEvent artifactStatusEvent)
            {
                var artifactCreated = new ArtifactCreated(artifactStatusEvent.Id, artifactStatusEvent.Key);

                var serialized = JsonSerializer.SerializeToElement(artifactCreated);

                yield return new WorkflowResponse(WorkflowState.Executing, artifactStatusEvent.Key, WorkflowAction.ArtifactCreated, serialized);
            }

            if (evt is TravelWorkflowErrorEvent travelWorkflowErrorEvent)
            {
                logger.LogError(travelWorkflowErrorEvent.Exception, $"Travel Workflow Error:{travelWorkflowErrorEvent.Message}, {travelWorkflowErrorEvent.Description}");
                yield return new WorkflowResponse(WorkflowState.Error, "Travel Request has failed.", WorkflowAction.ReportError);
                yield break;
            }

            if (evt is RequestInfoEvent requestInfoEvent)
            {
                switch (State)
                {
                    case WorkflowState.Executing:
                    {
                        var response = requestInfoEvent.HandleRequestForUserInput();

                        State = WorkflowState.WaitingForUserInput;

                        yield return response;
                        yield break;
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

        yield return new WorkflowResponse(State, string.Empty, WorkflowAction.None);
    }
}



