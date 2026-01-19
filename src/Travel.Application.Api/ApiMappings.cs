
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Extensions;
using Travel.Application.Api.Models;
using Travel.Application.Api.Services;

namespace Travel.Application.Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
    private const string CreateSessionPath = "travel/plans/session";
    private const string GetSessionPath = "travel/sessions/{sessionId}";
    private const string GetTravelPlanPath = "travel/plans/{travelPlanId}";
    private const string UpdateTravelPlanPath = "travel/plans/{threadId}";

    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapPost(CreateSessionPath, CreateSession);
        api.MapGet(GetTravelPlanPath, GetTravelPlan);
        api.MapGet(GetSessionPath, GetSession);
        api.MapPost(UpdateTravelPlanPath, UpdateTravelPlan);

        return app;
    }

    private static async Task UpdateTravelPlan(
        [FromBody] TravelPlanUpdateDto travelPlanUpdateDto, 
        Guid threadId, 
        HttpContext context, 
        ITravelPlanService travelPlanService,
        ISessionService sessionService)
    {
        var session = await sessionService.Get(context.User.Id(), threadId);

        await travelPlanService.UpdateAsync(travelPlanUpdateDto, context.User.Id(), session.TravelPlanId);
    }

    private static async Task<Ok<SessionDto>> GetSession(Guid sessionId, ISessionService sessionService, HttpContext context)
    {
        var session = await sessionService.Get(context.User.Id(), sessionId);

        var dto = new SessionDto(session.ThreadId, session.TravelPlanId);

        return TypedResults.Ok(dto);
    }

    private static async Task<Ok<TravelPlanDto>> GetTravelPlan(HttpContext context, Guid travelPlanId,ISessionService sessionService, ITravelPlanService travelPlanService)
    {
        var session = await sessionService.Get(context.User.Id(), travelPlanId);
        
        var travelPlan = await travelPlanService.GetTravelPlan(context.User.Id(), session.TravelPlanId);

        var dto = new TravelPlanDto(
            travelPlan.Origin, 
            travelPlan.Destination, 
            travelPlan.StartDate, 
            travelPlan.EndDate, 
            travelPlan.FlightPlan.FlightOptionsStatus, 
            travelPlan.FlightPlan.UserFlightOptionStatus,
            travelPlan.TravelPlanStatus
            );

        return TypedResults.Ok(dto);
    }

    private static async Task<Ok<SessionDto>> CreateSession(
        ITravelPlanService travelPlanService,
        ISessionService sessionService,
        HttpContext context)
    {
        var id = await travelPlanService.CreateTravelPlan(context.User.Id());

        var session = await sessionService.Create(context.User.Id(), id);

        var dto = new SessionDto(session.ThreadId, session.TravelPlanId);

        return TypedResults.Ok(dto);
    }
}