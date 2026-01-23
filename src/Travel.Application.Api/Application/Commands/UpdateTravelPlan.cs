
using Infrastructure.Dto;
using MediatR;
using Travel.Application.Api.Dto;
using Travel.Application.Api.Infrastructure;
using Travel.Application.Api.Models.Flights;
using FlightOptionDto = Infrastructure.Dto.FlightOptionDto;


namespace Travel.Application.Api.Application.Commands;

public record UpdateTravelPlanCommand(Guid UserId, Guid SessionId, TravelPlanUpdateDto TravelPlanUpdateDto) : IRequest;

public record UpdateTravelPlanFlightSearchCommand(Guid UserId, Guid SessionId, FlightSearchDto flightSearchDto) : IRequest;

public class UpdateTravelPlanHandler(ITravelPlanRepository travelPlanRepository, ISessionRepository sessionRepository) : IRequestHandler<UpdateTravelPlanCommand>
{
    public async Task Handle(UpdateTravelPlanCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.LoadAsync(request.UserId, request.SessionId);

        var travelPlan = await travelPlanRepository.LoadAsync(request.UserId, session.TravelPlanId);

        if (!string.IsNullOrEmpty(request.TravelPlanUpdateDto.Origin))
            travelPlan.SetOrigin(request.TravelPlanUpdateDto.Origin);

        if (!string.IsNullOrEmpty(request.TravelPlanUpdateDto.Destination))
            travelPlan.SetDestination(request.TravelPlanUpdateDto.Destination);

        if (request.TravelPlanUpdateDto.StartDate.HasValue)
            travelPlan.SetStartDate(request.TravelPlanUpdateDto.StartDate.Value);

        if (request.TravelPlanUpdateDto.EndDate.HasValue)
            travelPlan.SetEndDate(request.TravelPlanUpdateDto.EndDate.Value);

        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);
    }
}

public class UpdateTravelPlanFlightSearchHandler(ITravelPlanRepository travelPlanRepository, ISessionRepository sessionRepository) : IRequestHandler<UpdateTravelPlanFlightSearchCommand>
{
    public async Task Handle(UpdateTravelPlanFlightSearchCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.LoadAsync(request.UserId, request.SessionId);

        var travelPlan = await travelPlanRepository.LoadAsync(request.UserId, session.TravelPlanId);

        var flightOption = request.flightSearchDto.DepartureFlightOptions.First();

        travelPlan.SelectFlightOption(MapFlightOption(flightOption));

        await travelPlanRepository.SaveAsync(travelPlan, request.UserId);
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

