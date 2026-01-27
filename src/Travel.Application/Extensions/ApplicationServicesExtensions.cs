using Microsoft.Extensions.DependencyInjection;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Services;

namespace Travel.Application.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHostedService<AzureStorageSeedService>();

        services.AddScoped<ILocationRepository, LocationRepository>();

        services.AddScoped<ITravelPlanRepository, TravelPlanPlanRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IFlightSearchService, FlightSearchService>();

        return services;
    }
}