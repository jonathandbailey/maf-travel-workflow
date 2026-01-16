using Agents.Services;
using Application.Users;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureStorageSeedSettings>((options) => configuration.GetSection("AzureStorageSeedSettings").Bind(options));

        services.AddSingleton<IA2AAgentServiceDiscovery, A2AAgentServiceDiscovery>();

        services.AddSingleton<IExecutionContextAccessor, ExecutionContextAccessor>();
   
        return services;
    }
}