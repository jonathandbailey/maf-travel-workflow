using Workflows;
using Workflows.Dto;

namespace Application.Services;

public interface ITravelWorkflowService
{
    Task<WorkflowResponse> PlanVacation(WorkflowRequest request);
}