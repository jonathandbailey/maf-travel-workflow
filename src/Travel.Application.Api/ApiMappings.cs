
using Microsoft.AspNetCore.Http.HttpResults;
using Travel.Application.Api.Dto;

namespace Travel.Application.Api;

public static class ApiMappings
{
    private const string ApiConversationsRoot = "api";
    private const string CreateSessionPath = "travel/plans/session";
  
    public static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup(ApiConversationsRoot);

        api.MapPost(CreateSessionPath, CreateSession);
    
        return app;
    }

    private static async Task<Ok<SessionDto>> CreateSession(
        HttpContext context)
    {
        var session = new SessionDto(Guid.NewGuid().ToString(), Guid.NewGuid());
        return TypedResults.Ok(session);
    }

  
}