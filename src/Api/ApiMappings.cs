using Api.Dto;
using Application.Services;
using Application.Workflows.Conversations.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api/conversations";

    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapPost("/", ConversationExchange);

        return app;
    }

    private static async Task<Ok<ConversationResponseDto>> ConversationExchange([FromBody] ConversationRequestDto requestDto, IApplicationService service, HttpContext context)
    {
        var response = await service.Execute(new ConversationRequest(requestDto.SessionId, requestDto.Message));
        
        return TypedResults.Ok(new ConversationResponseDto(response.Message, response.SessionId));
    }
}