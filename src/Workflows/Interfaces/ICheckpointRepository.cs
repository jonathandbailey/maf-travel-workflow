using Workflows.Dto;

namespace Workflows.Interfaces;

public interface ICheckpointRepository
{
    Task<List<StoreStateDto>> GetAsync(string runId);
    Task SaveAsync(Guid threadId, StoreStateDto storeState);
    Task<StoreStateDto> LoadAsync(Guid threadId, string checkpointId, string runId);
}