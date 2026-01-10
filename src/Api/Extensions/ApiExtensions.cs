using Api.Hub;
using Application.Interfaces;

namespace Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        services.AddSingleton<IUserStreamingService, UserStreamingService>();

        return services;
    }
}