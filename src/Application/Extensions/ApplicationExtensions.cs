using Api.Infrastructure.Settings;
using Application.Agents;
using Application.Agents.Repository;
using Application.Infrastructure;
using Application.Services;
using Application.Settings;
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


        services.AddScoped<IAgentFactory, AgentFactory>();
        services.AddScoped<IAgentTemplateRepository, AgentTemplateRepository>();
        services.AddScoped<IApplicationService, ApplicationService>();

        services.AddScoped<IAzureStorageRepository, AzureStorageRepository>();

        services.AddHostedService<AzureStorageSeedService>();

        services.AddAzureClients(azure =>
        {
            azure.AddBlobServiceClient(configuration.GetConnectionString("blobs"));
        });

        return services;
    }
}