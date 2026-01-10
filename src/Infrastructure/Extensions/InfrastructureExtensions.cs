using Application.Interfaces;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IAzureStorageRepository, AzureStorageRepository>();
        services.AddSingleton<ICheckpointRepository, CheckpointRepository>();

        services.AddSingleton<IArtifactRepository, ArtifactRepository>();

        services.AddSingleton<IWorkflowRepository, WorkflowRepository>();

        services.AddHostedService<AzureStorageSeedService>();


        return services;
    }
}