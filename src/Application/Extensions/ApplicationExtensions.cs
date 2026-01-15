using Agents;
using Agents.Middleware;
using Agents.Repository;
using Agents.Services;
using Application.Services;
using Application.Users;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Workflows;
using Workflows.Repository;
using Workflows.Services;
using AgentMemoryService = Application.Services.AgentMemoryService;
using IAgentMemoryService = Application.Services.IAgentMemoryService;


namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
       

        services.Configure<AzureStorageSeedSettings>((options) => configuration.GetSection("AzureStorageSeedSettings").Bind(options));

        services.AddSingleton<IAgentFactory, AgentFactory>();
        
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();
        
        services.AddSingleton<IAgentMemoryService, AgentMemoryService>();

        services.AddSingleton<IA2AAgentServiceDiscovery, A2AAgentServiceDiscovery>();

      
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();
        services.AddSingleton<IWorkflowRepository, WorkflowRepository>();

        services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

        services.AddSingleton<IAgentMemoryMiddleware, AgentMemoryMiddleware>();

        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();
        
        services.AddSingleton<ITravelWorkflowService, TravelWorkflowService>();

        return services;
    }
}