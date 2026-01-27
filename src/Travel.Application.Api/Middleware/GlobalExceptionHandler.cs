using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Travel.Application.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware, IExceptionHandler
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
            
            await Handle(context, exception, CancellationToken.None);
        }
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        await Handle(httpContext, exception, cancellationToken);

        return true;
    }


    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Application Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Applications Services are not available. Please try again later."
            }
        };
    }

  
    private static async Task Handle(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(exception);

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var problemDetailsJson = JsonSerializer.Serialize(problemDetails);
        await httpContext.Response.WriteAsync(problemDetailsJson, cancellationToken: cancellationToken);
    }
}