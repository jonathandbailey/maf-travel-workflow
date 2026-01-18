
using Microsoft.AspNetCore.Http.HttpResults;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Extensions;
using Travel.Application.Api.Services;

namespace Travel.Application.Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
    private const string CreateSessionPath = "travel/plans/session";
    private const string GetTravelPlanPath = "travel/plans/{travelPlanId}";

    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapPost(CreateSessionPath, CreateSession);
        api.MapGet(GetTravelPlanPath, GetTravelPlan);

        return app;
    }

    private static async Task<Ok<TravelPlanDto>> GetTravelPlan(HttpContext context, Guid travelPlanId, ITravelPlanService travelPlanService)
    {
        var travelPlan = await travelPlanService.GetTravelPlan(context.User.Id(), travelPlanId);

        var dto = new TravelPlanDto();

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