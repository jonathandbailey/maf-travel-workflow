using Api.Dto;
using Api.Extensions;
using Application.Infrastructure;
using Application.Services;
using Application.Users;
using Application.Workflows.Dto;
using Application.Workflows.ReWoo.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
    private const string GetFlightPlanPath = "plans/{sessionId}/flights";
    private const string GetHotelPlanPath = "plans/{sessionId}/hotels";

    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapPost("/conversations", ConversationExchange);
        api.MapGet(GetFlightPlanPath, GetFlightPlan);
        api.MapGet(GetHotelPlanPath, GetHotelPlan);

        return app;
    }

    private static async Task<Ok<ConversationResponseDto>> ConversationExchange(
        [FromBody] ConversationRequestDto requestDto, 
        IApplicationService service,
        ISessionContextAccessor sessionContextAccessor,
        HttpContext context)
    {
        sessionContextAccessor.Initialize(context.User.Id(), requestDto.SessionId);
        
        var response = await service.Execute(new ConversationRequest(requestDto.SessionId, context.User.Id(), requestDto.Message, requestDto.ExchangeId));
        
        return TypedResults.Ok(new ConversationResponseDto(response.Message, response.SessionId, response.ExchangeId));
    }

    private static async Task<Ok<FlightSearchResultDto>> GetFlightPlan(
        Guid sessionId, 
        IArtifactRepository service,
        ISessionContextAccessor sessionContextAccessor,
        HttpContext context)
    {
        sessionContextAccessor.Initialize(context.User.Id(), sessionId);

        var flightPlan = await service.GetFlightPlanAsync();
        return TypedResults.Ok(flightPlan);
    }

    private static async Task<Ok<HotelSearchResultDto>> GetHotelPlan(
        Guid sessionId,
        IArtifactRepository service,
        ISessionContextAccessor sessionContextAccessor,
        HttpContext context)
    {
        sessionContextAccessor.Initialize(context.User.Id(), sessionId);

        var hotelPlan = await service.GetHotelPlanAsync();
        return TypedResults.Ok(hotelPlan);
    }
}