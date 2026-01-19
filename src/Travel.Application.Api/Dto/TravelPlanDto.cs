using Travel.Application.Api.Models;
using Travel.Application.Api.Models.Flights;

namespace Travel.Application.Api.Dto;

public class TravelPlanUpdateDto()
{
    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
  
}

public class TravelPlanDto(string? origin, string? destination, DateTimeOffset? startDate, DateTimeOffset? endDate, FlightOptionsStatus flightOptionsStatus, UserFlightOptionsStatus userFlightOptionStatus, TravelPlanStatus travelPlanStatus)
{
    public string? Origin { get;  } = origin;

    public string? Destination { get;  } = destination;

    public DateTimeOffset? StartDate { get;  } = startDate;

    public DateTimeOffset? EndDate { get;  } = endDate;

    public FlightOptionsStatus FlightOptionsStatus { get; private set; } = flightOptionsStatus;

  
    public UserFlightOptionsStatus UserFlightOptionStatus { get; private set; } = userFlightOptionStatus;

    public TravelPlanStatus TravelPlanStatus { get; private set; } = travelPlanStatus;
}


