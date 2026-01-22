using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Travel.Workflows.Dto;
using Travel.Workflows.Models;

namespace Travel.Workflows.Services;

public class TravelService : ITravelService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };
   

    private async Task<TravelPlanDto> GetTravelPlan(Guid threadId)
    {
        var httpClient = new HttpClient() { BaseAddress = new Uri("https://localhost:7010/")};

        var response = await httpClient.GetAsync($"/api/travel/plans/{threadId}");

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to retrieve travel plan: {response.ReasonPhrase}");

        var responseContent = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(responseContent))
            throw new HttpRequestException("Received empty response from travel plan endpoint");

        var travelPlanDto = JsonSerializer.Deserialize<TravelPlanDto>(responseContent, SerializerOptions);
   
        if( travelPlanDto == null)
            throw new InvalidOperationException("Failed to deserialize travel plan from response");

        return travelPlanDto;
    }


    public async Task<TravelPlanSummary> GetSummary(Guid threadId)
    {
        var travelPlanDto = await GetTravelPlan(threadId);
    
        var summaryEx = new TravelPlanSummary(travelPlanDto);

        return summaryEx;
    }

    public async Task UpdateTravelPlan(TravelPlanUpdateDto messageTravelPlanUpdate, Guid threadId)
    {
        var httpClient = new HttpClient() { BaseAddress = new Uri("https://localhost:7010/") };

        var content = new StringContent(JsonSerializer.Serialize(messageTravelPlanUpdate), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"/api/travel/plans/{threadId}", content);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to retrieve travel plan: {response.ReasonPhrase}");
    }
}

public interface ITravelService
{
    Task<TravelPlanSummary> GetSummary(Guid threadId);
    Task UpdateTravelPlan(TravelPlanUpdateDto messageTravelPlanUpdate, Guid threadId);
}