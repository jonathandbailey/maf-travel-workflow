using Aspire.Hosting.Azure;

namespace AppHost.Extensions;

public static class StorageServiceExtensions
{
    private const string StorageName = "storage";
    private const string StorageData = "../Storage/Data";
    private const string BlobStorageConnectionName = "blobs";
    public static IResourceBuilder<AzureStorageResource> AddAzureStorageServices(this IDistributedApplicationBuilder builder)
    {
        var storage = builder.AddAzureStorage(StorageName)
            .RunAsEmulator(resourceBuilder =>
                { resourceBuilder.WithDataBindMount(StorageData); });

        return storage;
    }

    public static IResourceBuilder<AzureBlobStorageResource> AddAzureBlobsServices(
        this IDistributedApplicationBuilder builder, IResourceBuilder<AzureStorageResource> storage)
    {
        var blobs = storage.AddBlobs(BlobStorageConnectionName);

        return blobs;
    }
}