using System.Text.Json.Serialization;
using Travel.Application.Api.Models.Flights;
using Travel.Workflows.Models;

namespace Travel.Application.Api.Models;

public class TravelPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    
    [JsonInclude]
    public string? Origin { get; private set; }
    
    [JsonInclude]
    public string? Destination { get; private set; }
    
    [JsonInclude]
    public DateTimeOffset? StartDate { get; private set; }

    [JsonInclude]
    public DateTimeOffset? EndDate { get; private set; }

    [JsonInclude]
    public TravelPlanStatus TravelPlanStatus { get; private set; } = TravelPlanStatus.NotStarted;

    [JsonInclude]
    public FlightPlan FlightPlan { get; private set; } = new();

    public TravelPlan() { }

    public void InProgress()
    {
        if (TravelPlanStatus == TravelPlanStatus.NotStarted)
        {
            TravelPlanStatus = TravelPlanStatus.InProgress;
        }
    }

    public void SetStartDate(DateTimeOffset startDate)
    {
        if (startDate != StartDate)
            StartDate = startDate;
    }
    public void SetEndDate(DateTimeOffset endDate)
    {
        if (endDate != EndDate)
            EndDate = endDate;
    }
    public void SetDestination(string destination)
    {
        ArgumentException.ThrowIfNullOrEmpty(destination);

        if (destination != Destination)
            Destination = destination;
    }

    public void SetOrigin(string origin)
    {
        ArgumentException.ThrowIfNullOrEmpty(origin);

        if (origin != Origin)
            Origin = origin;
    }

    public void AddFlightSearchOption(FlightOptionSearch flightOptions)
    {
        FlightPlan.AddFlightOptions(flightOptions);
    }

    public void SelectFlightOption(FlightOption flightOption)
    {
        FlightPlan.SelectFlightOption(flightOption);

        TravelPlanStatus = TravelPlanStatus.Completed;
    }
}