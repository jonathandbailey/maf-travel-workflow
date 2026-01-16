using Travel.Application.Api.Models;
using Travel.Workflows.Models;

namespace Travel.Application.Api.Dto;

public class TravelPlanSummary(TravelPlan plan)
{
    public string Origin { get; set; } = !string.IsNullOrEmpty(plan.Origin) ? plan.Origin : TravelPlanConstants.NotSet;

    public string Destination { get; set; } = !string.IsNullOrEmpty(plan.Destination) ? plan.Destination : TravelPlanConstants.NotSet;

    public string StartDate { get; set; } = plan.StartDate?.ToString("yyyy-MM-dd") ?? TravelPlanConstants.NotSet;

    public string EndDate { get; set; } = plan.EndDate?.ToString("yyyy-MM-dd") ?? TravelPlanConstants.NotSet;

    public string FlightOptionStatus { get; set; } = plan.FlightPlan.FlightOptionsStatus.ToString();

    public string UserFlightOptionStatus { get; set; } = plan.FlightPlan.UserFlightOptionStatus.ToString();

    public string TravelPlanStatus { get; set; } = plan.TravelPlanStatus.ToString();
}