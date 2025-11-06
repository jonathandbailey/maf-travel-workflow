using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Application.Infrastructure;

public class AzureStorageRepository(BlobServiceClient blobServiceClient, ILogger<AzureStorageRepository> logger)
    : IAzureStorageRepository
{
    public async Task UploadTextBlobAsync(string blobName, string containerName, string content, string contentType)
    {
        Verify.NotNullOrWhiteSpace(blobName);
        Verify.NotNullOrWhiteSpace(containerName);
        Verify.NotNullOrWhiteSpace(content);
        Verify.NotNullOrWhiteSpace(contentType);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        try
        {
            await using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            if (!IsTextContentType(contentType))
            {
                throw new InvalidOperationException(
                    $"Blob '{blobName}' in container '{containerName}' is not a supported text type. ContentType: {contentType}");
            }

            var blobUploadOptions = new BlobUploadOptions
                { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };

            await blobClient.UploadAsync(memoryStream, blobUploadOptions );
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Cannot upload blob, {blobName}, that are not text, from {containerName}", blobName, containerName);
            throw;
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception,
                "Failed to upload text blob {blobName} from blob storage container {containerName} : {errorCode}",
                blobName, containerName, exception.ErrorCode);
            throw;

        }
        catch (Exception exception)
        {
            logger.LogError(exception,"An unknown Exception when trying to upload to {blobName}, {containerName}.", blobName, containerName);
            throw;
        }
    }

    public async Task<bool> ContainerExists(string containerName)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        try
        {
            return await blobContainerClient.ExistsAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Error while checking if {containerName} exists on Azure Storage");
            throw;
        }
    }

    public async Task<bool> BlobExists(string blobName, string containerName)
    {
        Verify.NotNullOrWhiteSpace(blobName);
        Verify.NotNullOrWhiteSpace(containerName);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        try
        {
            return await blobClient.ExistsAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while checking if blob {blobName} exists in container {containerName}", blobName, containerName);
            throw;
        }
    }

    public async Task<string> DownloadTextBlobAsync(string blobName, string containerName)
    {
        Verify.NotNullOrWhiteSpace(blobName);
        Verify.NotNullOrWhiteSpace(containerName);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        try
        {
            var exists = await blobClient.ExistsAsync();
            
            if (!exists.Value)
            {
                throw new FileNotFoundException($"Blob '{blobName}' does not exist in container '{containerName}'");
            }

            var properties = await blobClient.GetPropertiesAsync();

            if (!IsTextContentType(properties.Value.ContentType))
            {
                throw new InvalidOperationException(
                    $"Blob '{blobName}' in container '{containerName}' is not a supported text type. ContentType: {properties.Value.ContentType}");
            }

            await using var memoryStream = new MemoryStream();

            await blobClient.DownloadToAsync(memoryStream);
            memoryStream.Position = 0;

            using var reader = new StreamReader(memoryStream);

            return await reader.ReadToEndAsync();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "Cannot download blob, {blobName}, that are not text, from {containerName}", blobName, containerName);
            throw;
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception,
                "Azure Request Failed to get  text blob {blobName} from blob storage container {containerName} : {errorCode}",
                blobName, containerName, exception.ErrorCode);
            throw;

        }
        catch (FileNotFoundException exception)
        {
            logger.LogError(exception, "Blob not found: {blobName} in container {containerName}", blobName, containerName);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Unknown Exception trying to get text blob {blobName} from blob storage container {containerName}", blobName, containerName);

            throw;
        }
    }

    private static bool IsTextContentType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        var lowerContentType = contentType.ToLowerInvariant();
     
        var textApplicationTypes = new[]
        {
            "text/plain",
            "application/json",
            "application/xml",
            "application/yaml",
            "application/x-yaml",
            "application/javascript",
            "application/csv"
        };

        return textApplicationTypes.Any(type => lowerContentType.Contains(type));
    }

    public async Task CreateContainerAsync(string containerName)
    {
        Verify.NotNullOrWhiteSpace(containerName);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        try
        {
            await blobContainerClient.CreateIfNotExistsAsync();
            logger.LogInformation("Container {containerName} created or already exists", containerName);
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception, "Failed to create container {containerName}: {errorCode}",
                containerName, exception.ErrorCode);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unknown exception occurred while creating container {containerName}",
                containerName);
            throw;
        }
    }
}

public interface IAzureStorageRepository
{
    Task<string> DownloadTextBlobAsync(string blobName, string containerName);
    Task UploadTextBlobAsync(string blobName, string containerName, string content, string contentType);
    Task<bool> ContainerExists(string containerName);
    Task<bool> BlobExists(string blobName, string containerName);
    Task CreateContainerAsync(string containerName);
}