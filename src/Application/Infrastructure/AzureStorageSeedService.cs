using Api.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Infrastructure;

public class AzureStorageSeedService(
    IServiceProvider serviceProvider,
    IOptions<AzureStorageSeedSettings> settings,
    ILogger<AzureStorageSeedService> logger) : IHostedService
{
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var storageRepository = scope.ServiceProvider.GetRequiredService<IAzureStorageRepository>();

        var containerName = settings.Value.ContainerName;
        

        try
        {
            var containerExists = await storageRepository.ContainerExists(containerName);

            if (!containerExists)
            {
                logger.LogInformation("Container '{ContainerName}' does not exist. Creating it now.", containerName);
                await storageRepository.CreateContainerAsync(containerName);
            }
           
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during seed data upload to container '{ContainerName}'", containerName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}