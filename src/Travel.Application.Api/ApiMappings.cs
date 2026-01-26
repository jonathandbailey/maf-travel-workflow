using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Travel.Application.Api.Application.Commands;
using Travel.Application.Api.Application.Queries;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Extensions;

namespace Travel.Application.Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
 
    public static void MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapSessionApi();
        api.MapTravelApi();
        api.MapFlightApi();
    }
}

public static class FlightsApiMappings
{
    private const string TravelFlightsSearchPath = "travel/flights/search/{threadId}";
    private const string TravelPlanFlightsSearchPath = "travel/flights/search/{threadId}";
    private const string GetTravelFlightsSearchPath = "travel/flights/search/{id}";

    public static void MapFlightApi(this RouteGroupBuilder api)
    {
        api.MapPost(TravelFlightsSearchPath, SaveFlightSearch);
        api.MapGet(GetTravelFlightsSearchPath, GetFlightSearch);

        api.MapPut(TravelPlanFlightsSearchPath, SaveFlightSearchToTravelPlan);
    }

    private static async Task<Ok<FlightSearchDto>> GetFlightSearch(Guid id, IMediator mediator, HttpContext context)
    {
        var result = await mediator.Send(new GetFlightSearchQuery(context.User.Id(), id));
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Guid>> SaveFlightSearch(
        [FromBody] FlightSearchDto flightSearch,
        Guid threadId,
        IMediator mediator,
        HttpContext context)
    {
        var id = await mediator.Send(new CreateFlightSearchCommand(context.User.Id(), threadId, flightSearch));

        return TypedResults.Ok(id);
    }

    private static async Task<Ok> SaveFlightSearchToTravelPlan(
        [FromBody] FlightSearchDto flightSearch,
        Guid threadId,
        IMediator mediator,
        HttpContext context)
    {

        await mediator.Send(new UpdateTravelPlanFlightSearchCommand(context.User.Id(), threadId, flightSearch));

        return TypedResults.Ok();
    }
}

public static class SessionsApiMappings
{
    private const string CreateSessionPath = "travel/plans/session";
    private const string GetSessionPath = "travel/sessions/{sessionId}";

    public static void MapSessionApi(this RouteGroupBuilder api)
    {
        api.MapPost(CreateSessionPath, CreateSession);
        api.MapGet(GetSessionPath, GetSession);
    }

    private static async Task<Ok<SessionDto>> GetSession(Guid sessionId, IMediator mediator, HttpContext context)
    {
        var sessionDto = await mediator.Send(new GetSessionQuery(context.User.Id(), sessionId));

        return TypedResults.Ok(sessionDto);
    }

    private static async Task<Ok<SessionDto>> CreateSession(IMediator mediator, HttpContext context)
    {
        var session = await mediator.Send(new CreateSessionCommand(context.User.Id()));

        return TypedResults.Ok(session);
    }
}   



public static class TravelApiMappings
{
    private const string GetTravelPlanPath = "travel/plans/{travelPlanId}";
    private const string UpdateTravelPlanPath = "travel/plans/{threadId}";

    public static void MapTravelApi(this RouteGroupBuilder api)
    {
        api.MapGet(GetTravelPlanPath, GetTravelPlan);
        api.MapPost(UpdateTravelPlanPath, UpdateTravelPlan);
    }

    private static async Task UpdateTravelPlan(
        [FromBody] TravelPlanUpdateDto travelPlanUpdateDto,
        Guid threadId,
        HttpContext context,
        IMediator mediator)
    {
        await mediator.Send(new UpdateTravelPlanCommand(context.User.Id(), threadId, travelPlanUpdateDto));
    }

    private static async Task<Ok<TravelPlanDto>> GetTravelPlan(HttpContext context, Guid travelPlanId, IMediator mediator)
    {
        var travelPlanDto = await mediator.Send(new GetTravelPlanQuery(context.User.Id(), travelPlanId));

        return TypedResults.Ok(travelPlanDto);
    }
}