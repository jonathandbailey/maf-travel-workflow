using Agents.Services;
using Microsoft.Extensions.DependencyInjection;
using Workflows.Repository;
using Workflows.Services;

namespace Workflows.Extensions;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();
        services.AddSingleton<IWorkflowRepository, WorkflowRepository>();
        services.AddSingleton<ITravelPlanService, TravelPlanService>();
        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();

        services.AddSingleton<IA2AAgentServiceDiscovery, A2AAgentServiceDiscovery>();

        return services;
    }
}