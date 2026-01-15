using Agents;
using Agents.Middleware;
using Agents.Repository;
using Application.Services;
using Application.Users;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workflows;
using Workflows.Repository;
using Workflows.Services;


namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
       

        services.Configure<AzureStorageSeedSettings>((options) => configuration.GetSection("AzureStorageSeedSettings").Bind(options));

        services.AddSingleton<IAgentFactory, AgentFactory>();
        
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();
        services.AddSingleton<IApplicationService, ApplicationService>();
        
        services.AddSingleton<IAgentMemoryService, AgentMemoryService>();

      
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();
        services.AddSingleton<IWorkflowRepository, WorkflowRepository>();

        services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

        services.AddSingleton<IAgentMemoryMiddleware, AgentMemoryMiddleware>();

        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();
        
        services.AddSingleton<ITravelWorkflowService, TravelWorkflowService>();

        return services;
    }
}