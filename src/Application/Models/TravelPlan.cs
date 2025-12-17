using Application.Models.Flights;
using System.Text.Json.Serialization;

namespace Application.Models;

public class TravelPlan
{
    [JsonInclude]
    public string? Origin { get; private set; }
    
    [JsonInclude]
    public string? Destination { get; private set; }
    
    [JsonInclude]
    public DateTime? StartDate { get; private set; }
    
    [JsonInclude]
    public DateTime? EndDate { get; private set; }

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

    public void SetStartDate(DateTime startDate)
    {
        if (startDate != StartDate)
            StartDate = startDate;
    }
    public void SetEndDate(DateTime endDate)
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