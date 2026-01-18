using System.Text.Json;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Travel.Application.Api.Models;

namespace Travel.Application.Api.Services;

public class SessionService(IAzureStorageRepository azureStorageRepository, IOptions<AzureStorageSeedSettings> settings) : ISessionService
{
    private const string ApplicationJsonContentType = "application/json";

    public async Task<Session> Create(Guid userId, Guid travelPlanId)
    {
        var session = new Session(userId, travelPlanId);

        var serializedSession = JsonSerializer.Serialize(session);

        await azureStorageRepository.UploadTextBlobAsync
            (GetResource(userId, session.ThreadId), settings.Value.ContainerName, serializedSession, ApplicationJsonContentType);

        return await Task.FromResult(session);
    }

    private string GetResource(Guid userId, Guid threadId)
    {
        return $"{userId}/sessions/{threadId}.json";
    }
}

public interface ISessionService
{
    Task<Session> Create(Guid userId, Guid travelPlanId);
}