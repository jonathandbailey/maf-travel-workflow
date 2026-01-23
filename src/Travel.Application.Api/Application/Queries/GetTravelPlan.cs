using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Models.Flights;
using Travel.Application.Api.Services;

namespace Travel.Application.Api.Application.Queries;

public record GetTravelPlanQuery(Guid User, Guid TravelPlanId) : IRequest<TravelPlanDto>;

public class GetTravelPlanHandler(ITravelPlanService travelPlanService, ISessionService sessionService)
    : IRequestHandler<GetTravelPlanQuery, TravelPlanDto>
{
    public async Task<TravelPlanDto> Handle(GetTravelPlanQuery request, CancellationToken cancellationToken)
    {
        var session = await sessionService.Get(request.User, request.TravelPlanId);

        var travelPlan = await travelPlanService.LoadAsync(request.User, session.TravelPlanId);

        var dto = new TravelPlanDto(
            travelPlan.Origin,
            travelPlan.Destination,
            travelPlan.StartDate,
            travelPlan.EndDate,
            travelPlan.FlightPlan.FlightOptionsStatus,
            travelPlan.FlightPlan.UserFlightOptionStatus,
            travelPlan.TravelPlanStatus,

            travelPlan.Id,
            MapFlightPlan(travelPlan.FlightPlan)
        );

        return dto;
    }

    private static FlightPlanDto MapFlightPlan(FlightPlan flightPlan)
    {
        if (flightPlan?.FlightOption == null)
        {
            return null;
        }

        return new FlightPlanDto
        {
            FlightOption = new FlightOptionDto
            {
                Airline = flightPlan.FlightOption.Airline,
                FlightNumber = flightPlan.FlightOption.FlightNumber,
                Departure = new FlightEndpointDto
                {
                    Airport = flightPlan.FlightOption.Departure.Airport,
                    Datetime = flightPlan.FlightOption.Departure.Datetime
                },
                Arrival = new FlightEndpointDto
                {
                    Airport = flightPlan.FlightOption.Arrival.Airport,

                    Datetime = flightPlan.FlightOption.Arrival.Datetime
                },
                Duration = flightPlan.FlightOption.Duration,
                Price = new PriceDto
                {
                    Amount = flightPlan.FlightOption.Price.Amount,
                    Currency = flightPlan.FlightOption.Price.Currency
                }
            }
        };
    }
}