using Travel.Application.Api.Models;
using Travel.Application.Api.Models.Flights;

namespace Travel.Application.Api.Infrastructure;

public class TravelPlanDocument
{
    public Guid Id { get; set; }
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public TravelPlanStatus TravelPlanStatus { get; set; }
    public FlightPlanDocument FlightPlan { get; set; } = new();
}

public class FlightPlanDocument
{
    public FlightOptionsStatus FlightOptionsStatus { get; set; }
    public UserFlightOptionsStatus UserFlightOptionStatus { get; set; }
    public List<FlightOptionSearchDocument> FlightOptions { get; set; } = [];
    public FlightOptionDocument? FlightOption { get; set; }
}

public class FlightOptionSearchDocument
{
    public Guid Id { get; set; }
}

public class FlightOptionDocument
{
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public FlightEndpointDocument Departure { get; set; } = new();
    public FlightEndpointDocument Arrival { get; set; } = new();
    public string Duration { get; set; } = string.Empty;
    public FlightPriceDocument Price { get; set; } = new();
}

public class FlightEndpointDocument
{
    public string Airport { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}

public class FlightPriceDocument
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}