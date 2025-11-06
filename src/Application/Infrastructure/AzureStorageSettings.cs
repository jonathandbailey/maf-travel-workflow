namespace Api.Infrastructure.Settings;

public class AzureStorageSeedSettings
{
    public required string ContainerName { get; init; }

    public required string LocalFolderPath { get; init; }
}