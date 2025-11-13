using Microsoft.Agents.AI.Workflows;

namespace Application.Workflows.ReAct.Dto;

public class WorkflowStateDto(WorkflowState state, CheckpointInfo? checkpointInfo)
{
    public WorkflowState State { get; } = state;

    public CheckpointInfo? CheckpointInfo { get; } = checkpointInfo;
}