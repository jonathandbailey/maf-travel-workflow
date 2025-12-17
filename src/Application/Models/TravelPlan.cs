using Application.Models.Flights;

namespace Application.Models;

public class TravelPlan
{
    public string? Origin { get; set; }
    public string? Destination { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public TravelPlanStatus TravelPlanStatus { get; set; } = TravelPlanStatus.NotStarted;

    public FlightPlan FlightPlan { get; init; } = new();

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