using Application.Infrastructure;
using Application.Users;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Models;
using Application.Workflows.Dto;

namespace Application.Services;

public interface ITravelPlanService
{
    Task SaveAsync(TravelPlan state);
    Task<bool> ExistsAsync();
    Task<TravelPlan> LoadAsync();
    Task<string> GetSummary();
    Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate);
}

public class TravelPlanService(IAzureStorageRepository repository, ISessionContextAccessor sessionContextAccessor, IOptions<AzureStorageSeedSettings> settings) : ITravelPlanService
{
    private const string ApplicationJsonContentType = "application/json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<string> GetSummary()
    {
        var travelPlan = await LoadAsync();

        var summary = new TravelPlanSummary(travelPlan);

        var serialized = JsonSerializer.Serialize(summary);

        if (serialized == null)
            throw new JsonException("Could not serialize the Travel Plan Summary");

        return serialized;
    }

    public async Task UpdateAsync(TravelPlanUpdateDto messageTravelPlanUpdate)
    {
        var travelPlan = await LoadAsync();

        if (messageTravelPlanUpdate.Origin != null)
            travelPlan.Origin = messageTravelPlanUpdate.Origin;

        if (messageTravelPlanUpdate.Destination != null)
            travelPlan.Destination = messageTravelPlanUpdate.Destination;
        if (messageTravelPlanUpdate.StartDate != null)
            travelPlan.StartDate = messageTravelPlanUpdate.StartDate;
        if (messageTravelPlanUpdate.EndDate != null)
            travelPlan.EndDate = messageTravelPlanUpdate.EndDate;

        await SaveAsync(travelPlan);
    }

    public async Task SaveAsync(TravelPlan state)
    {
        var serializedConversation = JsonSerializer.Serialize(state, SerializerOptions);

        await repository.UploadTextBlobAsync(
            GetStorageFileName(),
            settings.Value.ContainerName,
            serializedConversation,
            ApplicationJsonContentType);
    }

    public async Task<bool> ExistsAsync()
    {
        return await repository.BlobExists(GetStorageFileName(), settings.Value.ContainerName);
    }

    public async Task<TravelPlan> LoadAsync()
    {
        var blob = await repository.DownloadTextBlobAsync(GetStorageFileName(), settings.Value.ContainerName);

        var stateDto = JsonSerializer.Deserialize<TravelPlan>(blob, SerializerOptions);

        if (stateDto == null)
            throw new JsonException($"Failed to deserialize Travel Plan for session.");


        return stateDto;
    }


    private string GetStorageFileName()
    {
        var userId = sessionContextAccessor.Context.UserId;
        var sessionId = sessionContextAccessor.Context.SessionId;

        return $"{userId}/{sessionId}/plans/travel-plan.json";
    }
}