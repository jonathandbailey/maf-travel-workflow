
using Infrastructure.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Extensions;
using Travel.Application.Api.Models.Flights;
using Travel.Application.Api.Services;

namespace Travel.Application.Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
    private const string CreateSessionPath = "travel/plans/session";
    private const string GetSessionPath = "travel/sessions/{sessionId}";
    private const string GetTravelPlanPath = "travel/plans/{travelPlanId}";
    private const string UpdateTravelPlanPath = "travel/plans/{threadId}";
    private const string TravelFlightsSearchPath = "travel/flights/search/{threadId}";
    private const string TravelPlanFlightsSearchPath = "travel/flights/search/{threadId}";
    private const string GetTravelFlightsSearchPath = "travel/flights/search/{id}";

    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapPost(CreateSessionPath, CreateSession);
        api.MapGet(GetTravelPlanPath, GetTravelPlan);
        api.MapGet(GetSessionPath, GetSession);
        api.MapPost(UpdateTravelPlanPath, UpdateTravelPlan);
        api.MapPost(TravelFlightsSearchPath, SaveFlightSearch);
        api.MapGet(GetTravelFlightsSearchPath, GetFlightSearch);

        api.MapPut(TravelPlanFlightsSearchPath, SaveFlightSearchToTravelPlan);

        return app;
    }

    private static async Task<Ok<FlightSearchResultDto>> GetFlightSearch(
        Guid id,
        IFlightService flightService,
        HttpContext context)
    {
        var result = await flightService.GetFlightSearch(context.User.Id(), id);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<Guid>> SaveFlightSearch(
        [FromBody] FlightSearchResultDto flightSearch,
        Guid threadId,
        IFlightService flightService,
        ITravelPlanService travelPlanService,
        ISessionService sessionService,
        HttpContext context)
    {
        var id = await flightService.SaveFlightSearch(flightSearch);

        var session = await sessionService.Get(context.User.Id(), threadId);

        await travelPlanService.UpdateFlightSearchOption(context.User.Id(), session.TravelPlanId, id);

        return TypedResults.Ok(id);
    }

    private static async Task<Ok> SaveFlightSearchToTravelPlan(
        [FromBody] FlightSearchResultDto flightSearch,
        Guid threadId,
        IFlightService flightService,
        ITravelPlanService travelPlanService,
        ISessionService sessionService,
        HttpContext context)
    {
       
        var session = await sessionService.Get(context.User.Id(), threadId);

        var flightOption = flightSearch.DepartureFlightOptions.First();

        var mapped = MapFlightOption(flightOption);

        await travelPlanService.SaveFlightOption(context.User.Id(), session.TravelPlanId, mapped);

        return TypedResults.Ok();
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
        
        var travelPlan = await travelPlanService.LoadAsync(context.User.Id(), session.TravelPlanId);

        var dto = new TravelPlanDto(
            travelPlan.Origin, 
            travelPlan.Destination, 
            travelPlan.StartDate, 
            travelPlan.EndDate, 
            travelPlan.FlightPlan.FlightOptionsStatus, 
            travelPlan.FlightPlan.UserFlightOptionStatus,
            travelPlan.TravelPlanStatus,
            travelPlan.Id
            );

        return TypedResults.Ok(dto);
    }

    private static async Task<Ok<SessionDto>> CreateSession(
        ITravelPlanService travelPlanService,
        ISessionService sessionService,
        HttpContext context)
    {
        var id = await travelPlanService.CreateAsync(context.User.Id());

        var session = await sessionService.Create(context.User.Id(), id);

        var dto = new SessionDto(session.ThreadId, session.TravelPlanId);

        return TypedResults.Ok(dto);
    }

    private static FlightOption MapFlightOption(FlightOptionDto flightOption)
    {
        return new FlightOption
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = new FlightEndpoint
            {
                Airport = flightOption.Departure.Airport,
                Datetime = flightOption.Departure.Datetime
            },
            Arrival = new FlightEndpoint
            {
                Airport = flightOption.Arrival.Airport,
                Datetime = flightOption.Arrival.Datetime
            },
            Duration = flightOption.Duration,
            Price = new FlightPrice
            {
                Amount = flightOption.Price.Amount,
                Currency = flightOption.Price.Currency
            }
        };
    }
}