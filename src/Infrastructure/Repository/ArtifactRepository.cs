using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repository;

public class ArtifactRepository(IAzureStorageRepository repository, IOptions<AzureStorageSettings> settings) : IArtifactRepository
{
    private const string ApplicationJsonContentType = "application/json";

    public async Task SaveAsync(string artifact, Guid id, string path)
    {
        await repository.UploadTextBlobAsync(GetFlightSearchFileName(id, path),
            settings.Value.ContainerName,
            artifact, ApplicationJsonContentType);
    }

    public async Task<string> LoadAsync(Guid id, string path)
    {
        var filename = GetFlightSearchFileName(id, path);

        var response = await repository.DownloadTextBlobAsync(filename, settings.Value.ContainerName);

        return response;
    }

    private string GetFlightSearchFileName(Guid id, string path)
    {
        return $"artifacts/{path}/{id}.json";
    }
}