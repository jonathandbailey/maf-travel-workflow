using Api.Extensions;
using Application.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Workflows.Dto;
using Workflows.Models;
using Workflows.Services;

namespace Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
    private const string GetFlightPlanPath = "plans/{sessionId}/flights";
    private const string GetTravelPlanPath = "plans/{sessionId}/travel";

    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapGet(GetFlightPlanPath, GetFlightPlan);
        api.MapGet(GetTravelPlanPath, GetTravelPlan);

        return app;
    }

    private static async Task<Ok<TravelPlan>> GetTravelPlan(
        Guid sessionId, 
        ITravelPlanService service,
        IExecutionContextAccessor sessionContextAccessor,
        HttpContext context)
    {
        sessionContextAccessor.Initialize(context.User.Id(), sessionId);

        var travelPlan = await service.LoadAsync();
        return TypedResults.Ok(travelPlan);
    }

    private static async Task<Ok<FlightSearchResultDto>> GetFlightPlan(
        Guid sessionId, 
        ITravelPlanService service,
        IExecutionContextAccessor sessionContextAccessor,
        HttpContext context)
    {
        sessionContextAccessor.Initialize(context.User.Id(), sessionId);

        var flightPlan = await service.GetFlightOptionsAsync();
        return TypedResults.Ok(flightPlan);
    }

  
}