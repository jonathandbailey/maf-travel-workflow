using Microsoft.Extensions.Options;

namespace Application.Infrastructure;

public class ArtifactRepository(IAzureStorageRepository repository, IOptions<AzureStorageSeedSettings> settings) : IArtifactRepository
{
    private const string ApplicationJsonContentType = "application/json";

    public async Task SaveAsync(Guid sessionId, Guid userId, string artifact, string name)
    {
        await repository.UploadTextBlobAsync(GetCheckpointFileName(sessionId, userId, name),
            settings.Value.ContainerName,
            artifact, ApplicationJsonContentType);
    }

    private static string GetCheckpointFileName(Guid sessionId, Guid userId, string name)
    {
        return $"{userId}/{sessionId}/artifacts/{name}.json";
    }
}

public interface IArtifactRepository
{
    Task SaveAsync(Guid sessionId, Guid userId, string artifact, string name);
}