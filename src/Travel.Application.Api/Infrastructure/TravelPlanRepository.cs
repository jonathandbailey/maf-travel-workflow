using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Api.Models;

namespace Travel.Application.Api.Infrastructure;

public class TravelPlanPlanRepository(IAzureStorageRepository repository, IOptions<AzureStorageSettings> settings) : ITravelPlanRepository
{
    private const string ApplicationJsonContentType = "application/json";
    private const string StoragePathTemplate = "{0}/travel-plans/{1}.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId)
    {
        var blob = await repository.DownloadTextBlobAsync(GetStorageFileName(userId, travelPlanId), settings.Value.ContainerName);

        var document = JsonSerializer.Deserialize<TravelPlanDocument>(blob, SerializerOptions);

        if (document == null)
            throw new JsonException($"Failed to deserialize Travel Plan for session.");

        return document.ToDomain();
    }

    public async Task SaveAsync(TravelPlan travelPlan, Guid userId)
    {
        var serializedConversation = JsonSerializer.Serialize(travelPlan.ToDocument(), SerializerOptions);

        await repository.UploadTextBlobAsync(
            GetStorageFileName(userId, travelPlan.Id),
            settings.Value.ContainerName,
            serializedConversation,
            ApplicationJsonContentType);
    }

    private string GetStorageFileName(Guid userId, Guid travelPlanId)
    {
        return string.Format(StoragePathTemplate, userId, travelPlanId);
    }
}

public interface ITravelPlanRepository
{
    Task SaveAsync(TravelPlan state, Guid userId);
    Task<TravelPlan> LoadAsync(Guid userId, Guid travelPlanId);
}