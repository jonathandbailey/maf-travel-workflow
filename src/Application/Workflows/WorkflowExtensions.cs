using Application.Observability;
using Application.Workflows.ReAct;
using Application.Workflows.ReAct.Dto;
using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows;

public static class WorkflowExtensions
{
    public static async Task<Checkpointed<StreamingRun>> CreateStreamingRun<T>(this Workflow<T> workflow, T message, WorkflowState state, CheckpointManager checkpointManager, CheckpointInfo? checkpointInfo) where T : notnull
    {
        using var workflowActivity = Telemetry.Start("Workflow-[create-run]");

        workflowActivity?.SetTag("State:", state);

        switch (state)
        {
            case WorkflowState.Initialized:
                return await StartStreamingRun(workflow, message, checkpointManager);
            case WorkflowState.WaitingForUserInput:
                return await ResumeStreamingRun(workflow, checkpointInfo, checkpointManager);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static async Task<Checkpointed<StreamingRun>> StartStreamingRun<T>(Workflow<T> workflow, T message, CheckpointManager checkpointManager) where T : notnull
    {
        using var workflowActivity = Telemetry.Start("Workflow-[start]");


        return await InProcessExecution.StreamAsync(workflow, message, checkpointManager);
    }

    private static async Task<Checkpointed<StreamingRun>> ResumeStreamingRun<T>(Workflow<T> workflow,
        CheckpointInfo? checkpointInfo, CheckpointManager checkpointManager) where T : notnull
    {
        if (checkpointInfo == null)
            throw new ArgumentNullException(nameof(checkpointInfo),"Are CheckpointInfo is required to resume the workflow.");
        
        var run = await InProcessExecution.ResumeStreamAsync(workflow, checkpointInfo, checkpointManager,
            checkpointInfo.RunId);

        using var workflowActivity = Telemetry.Start("Workflow-[resume]");

        workflowActivity?.SetTag("RunId:", checkpointInfo.RunId);
        workflowActivity?.SetTag("CheckpointId:", checkpointInfo.CheckpointId);

        return run;

    }

    public static WorkflowResponse HandleRequestForUserInput(this RequestInfoEvent requestInfoEvent)
    {
        var data = requestInfoEvent.Data as ExternalRequest;

        if (data?.Data == null)
        {
            return new WorkflowResponse(WorkflowState.Error,
                "Invalid request event: missing data");
        }

        if (data.Data.AsType(typeof(UserRequest)) is not UserRequest userRequest)
        {
            return new WorkflowResponse(WorkflowState.Error,
                "Invalid request event: unable to parse UserRequest");
        }

        if (string.IsNullOrWhiteSpace(userRequest.Message))
        {
            return new WorkflowResponse(WorkflowState.Error,
                "Invalid request event: UserRequest message is empty");
        }

        return new WorkflowResponse(WorkflowState.WaitingForUserInput, userRequest.Message);
    }

}