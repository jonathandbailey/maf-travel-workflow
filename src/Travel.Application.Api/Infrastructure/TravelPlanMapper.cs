using Travel.Application.Api.Models;
using Travel.Application.Api.Models.Flights;

namespace Travel.Application.Api.Infrastructure;

public static class TravelPlanMapper
{
    public static TravelPlanDocument ToDocument(this TravelPlan travelPlan)
    {
        return new TravelPlanDocument
        {
            Id = travelPlan.Id,
            Origin = travelPlan.Origin,
            Destination = travelPlan.Destination,
            StartDate = travelPlan.StartDate,
            EndDate = travelPlan.EndDate,
            TravelPlanStatus = travelPlan.TravelPlanStatus,
            FlightPlan = travelPlan.FlightPlan.ToDocument()
        };
    }

    public static TravelPlan ToDomain(this TravelPlanDocument document)
    {
        var travelPlan = new TravelPlan();
        
        // Use reflection to set private properties since they have private setters
        var travelPlanType = typeof(TravelPlan);
        
        travelPlanType.GetProperty(nameof(TravelPlan.Id))?.SetValue(travelPlan, document.Id);
        travelPlanType.GetProperty(nameof(TravelPlan.Origin))?.SetValue(travelPlan, document.Origin);
        travelPlanType.GetProperty(nameof(TravelPlan.Destination))?.SetValue(travelPlan, document.Destination);
        travelPlanType.GetProperty(nameof(TravelPlan.StartDate))?.SetValue(travelPlan, document.StartDate);
        travelPlanType.GetProperty(nameof(TravelPlan.EndDate))?.SetValue(travelPlan, document.EndDate);
        travelPlanType.GetProperty(nameof(TravelPlan.TravelPlanStatus))?.SetValue(travelPlan, document.TravelPlanStatus);
        travelPlanType.GetProperty(nameof(TravelPlan.FlightPlan))?.SetValue(travelPlan, document.FlightPlan.ToDomain());
        
        return travelPlan;
    }

    private static FlightPlanDocument ToDocument(this FlightPlan flightPlan)
    {
        return new FlightPlanDocument
        {
            FlightOptionsStatus = flightPlan.FlightOptionsStatus,
            UserFlightOptionStatus = flightPlan.UserFlightOptionStatus,
            FlightOptions = flightPlan.FlightOptions.Select(fo => fo.ToDocument()).ToList(),
            FlightOption = flightPlan.FlightOption?.ToDocument()
        };
    }

    private static FlightPlan ToDomain(this FlightPlanDocument document)
    {
        var flightPlan = new FlightPlan();
        var flightPlanType = typeof(FlightPlan);
        
        flightPlanType.GetProperty(nameof(FlightPlan.FlightOptionsStatus))?.SetValue(flightPlan, document.FlightOptionsStatus);
        flightPlanType.GetProperty(nameof(FlightPlan.UserFlightOptionStatus))?.SetValue(flightPlan, document.UserFlightOptionStatus);
        flightPlanType.GetProperty(nameof(FlightPlan.FlightOptions))?.SetValue(flightPlan, document.FlightOptions.Select(fo => fo.ToDomain()).ToList());
        flightPlanType.GetProperty(nameof(FlightPlan.FlightOption))?.SetValue(flightPlan, document.FlightOption?.ToDomain());
        
        return flightPlan;
    }

    private static FlightOptionSearchDocument ToDocument(this FlightOptionSearch flightOptionSearch)
    {
        return new FlightOptionSearchDocument
        {
            Id = flightOptionSearch.Id
        };
    }

    private static FlightOptionSearch ToDomain(this FlightOptionSearchDocument document)
    {
        return new FlightOptionSearch(document.Id);
    }

    private static FlightOptionDocument ToDocument(this FlightOption flightOption)
    {
        return new FlightOptionDocument
        {
            Airline = flightOption.Airline,
            FlightNumber = flightOption.FlightNumber,
            Departure = flightOption.Departure.ToDocument(),
            Arrival = flightOption.Arrival.ToDocument(),
            Duration = flightOption.Duration,
            Price = flightOption.Price.ToDocument()
        };
    }

    private static FlightOption ToDomain(this FlightOptionDocument document)
    {
        return new FlightOption
        {
            Airline = document.Airline,
            FlightNumber = document.FlightNumber,
            Departure = document.Departure.ToDomain(),
            Arrival = document.Arrival.ToDomain(),
            Duration = document.Duration,
            Price = document.Price.ToDomain()
        };
    }

    private static FlightEndpointDocument ToDocument(this FlightEndpoint flightEndpoint)
    {
        return new FlightEndpointDocument
        {
            Airport = flightEndpoint.Airport,
            Time = flightEndpoint.Datetime
        };
    }

    private static FlightEndpoint ToDomain(this FlightEndpointDocument document)
    {
        return new FlightEndpoint
        {
            Airport = document.Airport,
            Datetime = document.Time
        };
    }

    private static FlightPriceDocument ToDocument(this FlightPrice flightPrice)
    {
        return new FlightPriceDocument
        {
            Amount = flightPrice.Amount,
            Currency = flightPrice.Currency
        };
    }

    private static FlightPrice ToDomain(this FlightPriceDocument document)
    {
        return new FlightPrice
        {
            Amount = document.Amount,
            Currency = document.Currency
        };
    }
}
