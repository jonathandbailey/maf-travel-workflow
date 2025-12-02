using Api.Hub;
using Application.Interfaces;

namespace Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IUserConnectionManager, UserConnectionManager>();
        services.AddScoped<IUserStreamingService, UserStreamingService>();
        
        return services;
    }
}