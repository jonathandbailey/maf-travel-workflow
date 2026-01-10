using Application.Agents;
using Application.Agents.Middleware;
using Application.Agents.Repository;
using Application.Services;
using Application.Settings;
using Application.Users;
using Application.Workflows;
using Infrastructure.Settings;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LanguageModelSettings>(settings => 
            configuration.GetSection("LanguageModelSettings").Bind(settings));

        services.Configure<AzureStorageSeedSettings>((options) => configuration.GetSection("AzureStorageSeedSettings").Bind(options));

        services.AddSingleton<IAgentFactory, AgentFactory>();
        
        services.AddSingleton<IAgentTemplateRepository, AgentTemplateRepository>();
        services.AddSingleton<IApplicationService, ApplicationService>();
        
        services.AddSingleton<IAgentMemoryService, AgentMemoryService>();

      
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();
    
        services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();

        services.AddSingleton<IAgentMemoryMiddleware, AgentMemoryMiddleware>();

        services.AddSingleton<ITravelPlanService, TravelPlanService>();
        
        services.AddSingleton<ITravelWorkflowService, TravelWorkflowService>();
   
        services.AddAzureClients(azure =>
        {
            azure.AddBlobServiceClient(configuration.GetConnectionString("blobs"));
        });

        return services;
    }
}