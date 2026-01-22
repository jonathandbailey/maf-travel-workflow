using Microsoft.Agents.AI.Workflows;
using Workflows.Dto;

namespace Workflows.Interfaces;

public interface IWorkflowRepository
{
    Task SaveAsync(Guid userId, Guid sessionId, WorkflowState state, CheckpointInfo? checkpointInfo);
   
    Task<WorkflowStateDto> LoadAsync(Guid userId, Guid sessionId);
    Task<WorkflowStateDto> LoadAsync(Guid threadId);
    Task SaveAsync(Guid threadId, WorkflowState state, CheckpointInfo? checkpointInfo);
}